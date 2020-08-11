using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public class EpubPage
    {
        public string Title;
        public string Language;
        public bool Vertical;
        public List<EpubLine> Lines;

        public override string ToString()
        {
            return Title;
        }

        public EpubPage(TextFile File)
        {
            Lines = new List<EpubLine>();
            Logger.Report(string.Format("Parsing page {0}", File.Name), LogType.Info);
            var doc = HTMLParser.Parse(File);

            var htmlNode = doc.DocumentNode.Element("html");
            if (htmlNode == null)
            {
                Logger.Report("html node not found, stopping parsing", LogType.Error);
                return;
            }

            var LangAttr = htmlNode.Attributes.FirstOrDefault(a => a.Name == "lang");
            if (LangAttr == null)
            {
                Logger.Report("language not found, orientation set to standard", LogType.Error);
                Vertical = false;
            }
            else
            {
                Language = LangAttr.Value;
                Vertical = GlobalSettings.IsVerticalLanguage(Language);
            }

            var HeadNode = htmlNode.Element("head");
            if (HeadNode == null)
            {
                Logger.Report("head node not found, title not set", LogType.Error);
            }
            else
            {
                var TitleNode = HeadNode.Element("title");
                if (TitleNode == null)
                {
                    Logger.Report("title node not found, title not set", LogType.Error);
                }
                else
                {
                    Title = TitleNode.InnerText;
                }
            }

            var BodyNode = htmlNode.Element("body");
            if (BodyNode == null)
            {
                Logger.Report("body node not found, lines not set", LogType.Error);
            }
            else
            {
                foreach (var Node in BodyNode.ChildNodes)
                {
                    if (Node.Name != "#text")
                    {
                        Lines.Add(new EpubLine(Node));
                    }
                }
            }
        }       
    }

    public class EpubLine
    {
        public override string ToString()
        {
            var Name = "";
            foreach (var Part in Parts)
            {
                Name += Part.Text;
            }
            return Name;
        }

        public List<LinePart> Parts;
        public EpubLine()
        {
            Parts = new List<LinePart>();
        }
        public EpubLine(HtmlNode HtmlLine)
        {
            Parts = new List<LinePart>();
            if (HtmlLine == null)
            {
                Logger.Report("node has value null", LogType.Error);
                return;
            }
            if (HtmlLine.Name == "p")
            {
                foreach (var Node in HtmlLine.ChildNodes)
                {
                    AddAppropriatePart(Node);
                }
            }
            else
            {
                AddAppropriatePart(HtmlLine);
            }
        }

        private void AddAppropriatePart(HtmlNode Node)
        {
            switch (Node.Name)
            {
                case "#text":
                    Parts.Add(new LinePart(Node.InnerHtml, ""));
                    break;
                case "ruby":
                    var Text = Node.ChildNodes[0].InnerHtml;
                    var Ruby = Node.ChildNodes[1].InnerHtml;
                    Parts.Add(new LinePart(Text, Ruby));
                    break;
                case "br":
                    Parts.Add(new LinePart("", ""));
                    break;
                case "span":
                    var NewPart = GetSpanElement(Node);
                    Parts.Add(NewPart);
                    break;
                default:
                    Logger.Report(string.Format("unknown element in line \"{0}\": \"{1}\""
                        , Node.ParentNode.OuterHtml, Node.Name), LogType.Error);
                    break;
            }
        }

        private LinePart GetSpanElement(HtmlNode node)
        {
            var NewPart = new LinePart("ERROR", "");
            var classAttribute = node.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttribute == null)
            {
                Logger.Report("span is missing class attribute: " + node.OuterHtml, LogType.Error);
            }
            else
            {
                switch (classAttribute.Value)
                {
                    case "sesame":
                        NewPart.Text = node.ChildNodes[0].InnerHtml;
                        NewPart.Format = TextFormat.sesame;
                        break;
                    case "img":
                        var SourceAttribute = node.ChildNodes[0].Attributes.FirstOrDefault(a => a.Name == "src");
                        if (SourceAttribute == null)
                        {
                            Logger.Report(string.Format("image source attribute not found in \"{0}\"",
                                node.OuterHtml), LogType.Error);
                        }
                        else
                        {
                            NewPart.Text = SourceAttribute.Value;
                            NewPart.Format = TextFormat.image;
                        }
                        break;
                    default:
                        Logger.Report("unknown span class: " + classAttribute.Value, LogType.Error);
                        break;
                }
            }
            return NewPart;
        }
    }

    public class LinePart
    {
        public string Text;
        public string Ruby;
        public TextFormat Format;

        public LinePart(string Text, string Ruby)
        {
            this.Text = Text;
            this.Ruby = Ruby;
            Format = TextFormat.none;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public enum TextFormat
    {
        none, sesame, image
    }
}
