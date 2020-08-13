using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public class EpubPage : IBaseFile
    {
        public string Title;
        public string Language;
        public bool Vertical;
        public List<EpubLine> Lines;

        public string Name { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return Title;
        }

        public EpubPage(TextFile File, BookSettings Settings)
        {
            Lines = new List<EpubLine>();
            Name = File.Name;
            FullName = File.FullName;
            Logger.Report(string.Format("Parsing page \"{0}\"", File.Name), LogType.Info);
            var doc = HTMLParser.Parse(File);

            var htmlNode = HTMLParser.SafeNodeGet(doc.DocumentNode, "html");
            if (htmlNode == null)
            {
                Logger.Report("stopping parsing", LogType.Error);
                return;
            }

            var LangAttr = HTMLParser.SafeAttributeGet(htmlNode, "lang");
            if (LangAttr == "")
            {
                Logger.Report("language not found, set to standard", LogType.Info);
                Vertical = Settings.StandardVertical;
                Language = Settings.Language;
            }
            else
            {
                Language = LangAttr;
                Vertical = GlobalSettings.IsVerticalLanguage(Language);
            }

            var HeadNode = HTMLParser.SafeNodeGet(htmlNode, "head");
            if (HeadNode != null)
            {
                var ParsedTitle = HTMLParser.SafeNodeTextGet(HeadNode, "title");
                if (ParsedTitle == "")
                {
                    Logger.Report("title not found, set to standard", LogType.Info);
                    Title = Settings.Title;
                }
                else
                {
                    Title = ParsedTitle;
                }
            }

            var BodyNode = HTMLParser.SafeNodeGet(htmlNode, "body");
            if (BodyNode != null)
            {
                foreach (var Node in BodyNode.ChildNodes)
                {
                    if (Node.Name != "#text")
                    {
                        var NewLine = new EpubLine(Node);
                        if (NewLine.Parts.Count > 0)
                        {
                            Lines.Add(new EpubLine(Node));
                        }
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
            AddAppropriatePart(HtmlLine);
        }

        private void AddAppropriatePart(HtmlNode Node)
        {
            switch (Node.Name)
            {
                case "#text":
                case "nav":
                    Parts.Add(new TextLinePart(Node.InnerHtml, ""));
                    break;
                case "hr":
                    Parts.Add(new SeparatorLinePart());
                    break;
                case "ruby":
                    var Text = Node.ChildNodes[0].InnerHtml;
                    var Ruby = Node.ChildNodes[1].InnerHtml;
                    Parts.Add(new TextLinePart(Text, Ruby));
                    break;
                case "br":
                    Parts.Add(new TextLinePart("", ""));
                    break;
                case "span":
                    AddSpanElement(Node);
                    break;
                case "a":
                    var Ref = HTMLParser.SafeAttributeGet(Node, "href");
                    if (Ref != "")
                    {
                        Parts.Add(new LinkLinePart(Node.InnerText, Ref));
                    }
                    break;
                case "p":
                case "svg":
                case "div":               
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode);
                    }
                    break;
                case "image":
                case "img":
                    string Link = "";
                    foreach (var ImageSourcAttribute in GlobalSettings.PossibleImageSourceNames)
                    {
                        Link = HTMLParser.SafeAttributeGet(Node, ImageSourcAttribute, true);
                        if (Link != "")
                            break;
                    }
                    if (Link == "")
                    {
                        Logger.Report("can't find link to image: " + Node.OuterHtml, LogType.Error);
                        break;
                    }
                    Parts.Add(new ImageLinePart(Link));
                    break;
                default:
                    Logger.Report(string.Format("unknown element \"{2}\" in \"{1}\" in line \"{0}\" "
                        , Node.OuterHtml, Node.ParentNode.Name, Node.Name), LogType.Error);
                    Logger.Report("trying to force parse...", LogType.Info);
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode);
                    }
                    break;
            }
        }

        private void AddSpanElement(HtmlNode node)
        {
            var classAttribute = HTMLParser.SafeAttributeGet(node, "class", true);

            switch (classAttribute)
            {
                case "sesame":
                    var NewSesamePart = new TextLinePart
                    {
                        Text = node.ChildNodes[0].InnerHtml,
                        Type = LinePartTypes.sesame
                    };
                    Parts.Add(NewSesamePart);
                    return;
                case "img":
                    foreach (var ChildNode in node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode);
                    }
                    return;
                default:

                    if (classAttribute != "")
                    {
                        bool Ignore = GlobalSettings.IgnoreableSpanClassParts.Any(a => classAttribute.Contains(a));
                        if (!Ignore)
                        {
                            Logger.Report(string.Format("unknown span class \"{0}\"," +
                                " trying to parse inner HTML...", classAttribute), LogType.Error);
                        }
                    }
                    foreach (var ChildNode in node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode);
                    }
                    return;
            }
        }
    }

    public class SeparatorLinePart : LinePart
    {
        public SeparatorLinePart()
        {
            this.Type = LinePartTypes.separator;
        }
    }

    public class LinkLinePart :LinePart
    {
        public string Link;

        public LinkLinePart(string Text, string Link)
        {
            this.Text = Text;
            this.Link = Link;
            this.Type = LinePartTypes.link;
        }
    }

    public class TextLinePart : LinePart
    {
        public string Ruby;

        public TextLinePart(string Text, string Ruby)
        {
            this.Text = Text;
            this.Ruby = Ruby;
            Type = LinePartTypes.normal;
        }

        public TextLinePart()
        {
            Type = LinePartTypes.normal;
        }
    }

    public class ImageLinePart : LinePart
    {
        public ZipEntry Image;

        public ImageLinePart(string Path)
        {
            this.Text = Path;
            this.Type = LinePartTypes.image;
        }

        public void SetImage(List<ZipEntry> Entries)
        {
            throw new NotImplementedException();
        }
    }


    public class LinePart
    {
        public string Text;
        public LinePartTypes Type;

        public override string ToString()
        {
            return Text;
        }
    }

    public enum LinePartTypes
    {
        normal, sesame, image, link, separator
    }
}
