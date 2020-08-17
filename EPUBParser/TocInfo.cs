using EPUBParser;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;

namespace EPUBParser
{
    public class TocInfo
    {
        public string Title;
        public List<string> Chapters;

        public TocInfo(ZipEntry file)
        {
            Chapters = new List<string>();

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
                var TextNode = navPointNode.Descendants("text").FirstOrDefault();
                if (TextNode == null)
                {
                    Logger.Report("text node not found, can't set chapter", LogType.Error);
                    continue;
                }
                var ChapterTitle = TextNode.InnerText;

                var OrderAttribute = navPointNode.Attributes.FirstOrDefault(a => a.Name == "playorder");
                if (OrderAttribute == null)
                {
                    Logger.Report(string.Format("can't determine order of chapter \"{0}\", " +
                        "appending at the end of current list", ChapterTitle), LogType.Error);
                    Chapters.Add(ChapterTitle);
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
                    Chapters.Add(ChapterTitle);
                    continue;
                }
                if (Chapters.Count >= Index)
                {
                    Chapters.Insert((int)Index, ChapterTitle);
                }
                else
                {
                    while (Chapters.Count < Index)
                    {
                        Chapters.Add(null);
                    }
                    Chapters.Insert((int)Index, ChapterTitle);
                }
                Chapters.RemoveAll(a => a == null);
            }
        }
    }
}