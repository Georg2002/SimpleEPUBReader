using EPUBParser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Pipes;
using System.Linq;

namespace EPUBParser
{
    public class TocInfo
    {
        public string Title;
        public List<ChapterDefinition> Chapters;

        public TocInfo(ZipEntry file, List<ZipEntry> files,bool fromNav = false)
        {
            Chapters = new List<ChapterDefinition>();
            if (file == null)
            {
                Logger.Report("file is null, can't parse chapters", LogType.Error);
                return;
            }

            if (fromNav) AddChaptersFromNav(file, files);
            else AddChaptersFromToc(file, files);      
            
        }

        private void AddChaptersFromToc(ZipEntry file, List<ZipEntry> files)
        {
            Logger.Report(string.Format("parsing toc at \"{0}\"", file.Name), LogType.Info);
            var doc = HTMLParser.Parse(file);
            var ncxNode = doc.DocumentNode.Element("ncx");
            if (ncxNode == null)
            {
                Logger.Report("ncx node doesn't exist", LogType.Error);
                return;
            }
            var docTitleNode = ncxNode.Element("doctitle");
            if (docTitleNode == null) Logger.Report("docTitle Node doesn't exist", LogType.Error);
            else
            {
                var docTitleText = docTitleNode.Element("text");
                if (docTitleText == null) Logger.Report("docTitle text doesn't exist", LogType.Error);
                else Title = docTitleText.InnerText;
            }

            var navMapNode = ncxNode.Element("navmap");
            if (navMapNode == null)
            {
                Logger.Report("navMap node doesn't exist, can't read chapters", LogType.Error);
                return;
            }
            var NavPointNodes = navMapNode.Elements("navpoint");
            if (NavPointNodes == null || NavPointNodes.Count() == 0)
            {
                Logger.Report("navPoints not found or don't exist, can't read chapters", LogType.Error);
                return;
            }
            foreach (var navPointNode in NavPointNodes)
            {
                var NewChapter = new ChapterDefinition();
                var TextNode = navPointNode.Descendants("text").FirstOrDefault();
                if (TextNode == null)
                {
                    Logger.Report("text node not found, can't set chapter", LogType.Error);
                    continue;
                }
                NewChapter.Title = TextNode.InnerText;

                var SourceNode = navPointNode.Descendants("content").FirstOrDefault();
                if (SourceNode == null)
                {
                    Logger.Report(string.Format("chapter source of chapter +" +
                        "\"{0}\" missing, skipping", NewChapter.Title), LogType.Error);
                    continue;
                }

                string Source = HTMLParser.SafeAttributeGet(SourceNode, "src");
                var Split = Source.Split('#');
                string Jumppoint = Split.Length == 1 ? "" : Split[1];
                Source = Split[0];
                string FullSource = "";
                ZipEntry Page = ZipEntry.GetEntryByPath(files, Source, file);
                if (Page != null) FullSource = Page.FullName;
                if (string.IsNullOrEmpty(FullSource))
                {
                    Logger.Report(string.Format("chapter source of chapter +" +
                      "\"{0}\" is empty, skipping", NewChapter.Title), LogType.Error);
                    continue;
                }
                NewChapter.Source = FullSource;
                NewChapter.Jumppoint = Jumppoint;

                var OrderAttribute = navPointNode.Attributes.FirstOrDefault(a => a.Name == "playorder");
                if (OrderAttribute == null)
                {
                    Logger.Report(string.Format("can't determine order of chapter \"{0}\", " +
                        "appending at the end of current list", NewChapter.Title), LogType.Error);
                    NewChapter.Index = Chapters.Max(a => a.Index) + 1;
                    continue;
                }
             
                try
                {
                    NewChapter.Index = Convert.ToInt32(OrderAttribute.Value);
                }
                catch (Exception)
                {
                    Logger.Report(string.Format("can't convert {0} to uint, appending chapter at the end of current list"
                        , OrderAttribute.Value), LogType.Error);
                    NewChapter.Index = Chapters.Max(a=>a.Index)+1;
                }
                InsertChapter(NewChapter);
            }
            Chapters.RemoveAll(a => a == null);
        }

        internal void AddChaptersFromNav(ZipEntry file, List<ZipEntry> files)
        {
            Logger.Report($"parsing nav at \"{file.Name}\"", LogType.Info);
            var doc = HTMLParser.Parse(file);
            var tocNode = doc.DocumentNode.SelectSingleNode("//nav[@id=\"toc\"]");
            if (tocNode == null)
            {
                Logger.Report("toc node doesn't exist", LogType.Error);
                return;
            }
            var linkNodes = tocNode.SelectNodes("//a");   
            foreach (var linkNode in linkNodes)
            {
                var NewChapter = new ChapterDefinition();
                NewChapter.Title = linkNode.InnerText;
                string source = HTMLParser.SafeAttributeGet(linkNode, "href");
                if (string.IsNullOrEmpty(source))
                {
                    Logger.Report($"chapter source of chapter +" +
                        "\"{NewChapter.Title}\" missing, skipping", LogType.Error);
                    continue;
                }

                var Split = source.Split('#');
                string Jumppoint = Split.Length == 1 ? "" : Split[1];
                source = Split[0];
                string FullSource = "";
                ZipEntry Page = ZipEntry.GetEntryByPath(files, source, file);
                if (Page != null) FullSource = Page.FullName;
                if (string.IsNullOrEmpty(FullSource))
                {
                    Logger.Report(string.Format("chapter source of chapter +" +
                      "\"{0}\" is empty, skipping", NewChapter.Title), LogType.Error);
                    continue;
                }
                NewChapter.Source = FullSource;
                NewChapter.Jumppoint = Jumppoint;
                NewChapter.Index = -1;
                InsertChapter(NewChapter);
            }
            Chapters.RemoveAll(a => a == null);
        }

        private void InsertChapter(ChapterDefinition newChapter )
        {
            bool insert = !Chapters.Exists(a => a.Source == newChapter.Source && a.Jumppoint == newChapter.Jumppoint);
            if (insert)
            {
                if (newChapter.Index==-1)
                {
                    Chapters.Add(newChapter);
                    return;
                }
                if (Chapters.Count >= newChapter.Index)
                {
                    Chapters.Insert(newChapter.Index, newChapter);
                }
                else
                {
                    while (Chapters.Count < newChapter.Index)
                    {
                        Chapters.Add(null);
                    }
                    Chapters.Insert(newChapter.Index, newChapter);
                }
            }
        }
    }

    public class ChapterDefinition
    {
        public string Source;
        public string Title;
        public string Jumppoint;
        public int Index;
    }
}