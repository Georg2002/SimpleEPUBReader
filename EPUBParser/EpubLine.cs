using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPUBParser
{
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

        public List<BaseLinePart> Parts;
        public EpubLine()
        {
            Parts = new List<BaseLinePart>();
        }
        public EpubLine(HtmlNode HtmlLine, List<ZipEntry> Entries, ZipEntry File)
        {
            Parts = new List<BaseLinePart>();
            if (HtmlLine == null)
            {
                Logger.Report("node has value null", LogType.Error);
                return;
            }
            var ActiveClasses = new List<string>();
            AddAppropriatePart(HtmlLine, Entries, File,ActiveClasses);
        }

        private void AddAppropriatePart(HtmlNode Node, List<ZipEntry> Entries, ZipEntry File, List<string> ActiveClasses)
        {
            string Text = "";
            string NewClass = HTMLParser.SafeAttributeGet(Node, "class", true);
            bool ClassAdded = false;
            if (!string.IsNullOrEmpty(NewClass))
            {
                ClassAdded = true;
                ActiveClasses.Add(NewClass);
            }
            switch (Node.Name)
            {
                case "#text":
                case "nav":
                    Text = Node.InnerText;
                    if (!string.IsNullOrWhiteSpace(Text))
                    {
                        Parts.Add(new TextLinePart(Text, "",ActiveClasses));
                    }
                    break;
                case "ruby":
                    if (Node.ChildNodes.Count >= 2)
                    {
                        string Ruby = "";
                        bool Broken = false;
                        foreach (var Child in Node.ChildNodes)
                        {
                            if (Broken) break;
                            void SwitchName(HtmlNode ChildNode)
                            {
                                switch (ChildNode.Name)
                                {
                                    case "rt":
                                        Ruby += Child.InnerText;
                                        break;
                                    case "#text":
                                    case "rb":
                                        Text += Child.InnerText;
                                        break;
                                    case "span":
                                        foreach (var SubChild in Child.ChildNodes)
                                        {
                                            SwitchName(SubChild);
                                        }
                                        break;
                                    default:
                                        Logger.Report("Broken ruby found, ignoring", LogType.Error);
                                        Broken = true;
                                        break;
                                }
                            }
                            SwitchName(Child);
                        }
                        Parts.Add(new TextLinePart(Text, Ruby, ActiveClasses));
                    }
                    else
                    {
                        Logger.Report("Broken ruby found, ignoring", LogType.Error);
                    }
                    break;
                case "hr":
                case "br":
                    Parts.Add(new BreakLinePart());
                    break;
                case "span":
                    AddSpanElement(Node, Entries, File, ActiveClasses);
                    break;
                case "a":
                case "svg":
                case "div":
                    AddChapterMarker(Node);
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, Entries, File, ActiveClasses);
                    }
                    break;
                case "p":
                    AddChapterMarker(Node);
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, Entries, File, ActiveClasses);
                    }
                    Parts.Add(new BreakLinePart());
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
                    var Inline = false;
                    var Parent = Node.ParentNode;
                    for (int i = 0; i < 20; i++)
                    {
                        Inline = Parent.Name == "p" && !string.IsNullOrWhiteSpace(Parent.InnerText);
                        if (Inline || Parent.ParentNode == null) break;                       
                        Parent = Parent.ParentNode;                         
                    }
                    var Image = new ImageLinePart(Link, Inline);
                    //Set later to allow parallelization
                    Parts.Add(Image);
                    break;
                default:
                    Logger.Report(string.Format("unknown element \"{2}\" in \"{1}\" in line \"{0}\""
                        , Node.OuterHtml, Node.ParentNode.Name, Node.Name), LogType.Error);
                    Logger.Report("trying to force parse...", LogType.Info);
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, Entries, File,ActiveClasses);
                    }
                    break;
            }

            if (ClassAdded) ActiveClasses.RemoveAt(ActiveClasses.Count - 1);           
        }

        private void AddChapterMarker(HtmlNode Node)
        {
            string Id = HTMLParser.SafeAttributeGet(Node, "id", true);
            if (!string.IsNullOrEmpty(Id)) Parts.Add(new ChapterMarkerLinePart(Id));
        }

        private void AddSpanElement(HtmlNode node, List<ZipEntry> Entries, ZipEntry File, List<string> ActiveClasses)
        {
            AddChapterMarker(node);
            var classAttribute = HTMLParser.SafeAttributeGet(node, "class", true);
            var IgnoreAttribute = HTMLParser.SafeAttributeGet(node, "data-amznremoved-m8", true);
            if (IgnoreAttribute == "true") return;

            switch (classAttribute)
            {
                case "sesame":
                    if (node.ChildNodes.Count == 0) return;
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
                        AddAppropriatePart(ChildNode, Entries, File, ActiveClasses);
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
                        AddAppropriatePart(ChildNode, Entries, File, ActiveClasses);
                    }
                    return;
            }
        }
    }
}
