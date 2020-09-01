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

        public TocInfo(ZipEntry file, List<ZipEntry> files)
        {
            Chapters = new List<ChapterDefinition>();

            if (file == null)
            {
                Logger.Report("file is null, can't parse toc file", LogType.Error);
                return;
            }

            Logger.Report(string.Format("parsing toc at \"{0}\"", file.Name), LogType.Info);
            var doc = HTMLParser.Parse(file);
            var ncxNode = doc.DocumentNode.Element("ncx");
            if (ncxNode == null)
            {
                Logger.Report("ncx node doesn't exist", LogType.Error);
                return;
            }
            var docTitleNode = ncxNode.Element("doctitle");
            if (docTitleNode == null)
            {
                Logger.Report("docTitle Node doesn't exist", LogType.Error);
            }
            else
            {
                var docTitleText = docTitleNode.Element("text");
                if (docTitleText == null)
                {
                    Logger.Report("docTitle text doesn't exist", LogType.Error);
                }
                else
                {
                    Title = docTitleText.InnerText;
                }
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
                Source = Source.Split('#')[0];
                string FullSource = "";
                ZipEntry Page = ZipEntry.GetEntryByPath(files, Source, file);
                if (Page != null)
                {
                    FullSource = ZipEntry.GetEntryByPath(files, Source, file).FullName;
                }               
                if (FullSource == "")
                {
                    Logger.Report(string.Format("chapter source of chapter +" +
                      "\"{0}\" is empty, skipping", NewChapter.Title), LogType.Error);
                    continue;
                }
                NewChapter.Source = FullSource;

                var OrderAttribute = navPointNode.Attributes.FirstOrDefault(a => a.Name == "playorder");
                if (OrderAttribute == null)
                {
                    Logger.Report(string.Format("can't determine order of chapter \"{0}\", " +
                        "appending at the end of current list", NewChapter.Title), LogType.Error);
                    Chapters.Add(NewChapter);
                    continue;
                }
                UInt32 Index;
                try
                {
                    Index = Convert.ToUInt32(OrderAttribute.Value);
                }
                catch (Exception)
                {
                    Logger.Report(string.Format("can't convert {0} to uint, appending chapter at the end of current list"
                        , OrderAttribute.Value), LogType.Error);
                    Chapters.Add(NewChapter);
                    continue;
                }
                if (Chapters.Count >= Index)
                {
                    Chapters.Insert((int)Index, NewChapter);
                }
                else
                {
                    while (Chapters.Count < Index)
                    {
                        Chapters.Add(null);
                    }
                    Chapters.Insert((int)Index, NewChapter);
                }
            }
            Chapters.RemoveAll(a => a == null);
        }
    }

    public class ChapterDefinition
    {
        public string Source;
        public string Title;
    }
}