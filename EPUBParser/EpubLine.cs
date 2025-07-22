using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPUBParser
{
    public class LineSplitInfo
    {
        public List<ZipEntry> Entries;
        public ZipEntry File;
        public List<string> ActiveClasses;
        public bool IsRuby;
        public bool Splittable;
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
            AddAppropriatePart(HtmlLine, new LineSplitInfo() { Entries = Entries, File = File, ActiveClasses = ActiveClasses, IsRuby = false, Splittable = true });
        }

        private void AddAppropriatePart(HtmlNode Node, LineSplitInfo info)
        {
            string Text = "";
            string NewClass = HTMLParser.SafeAttributeGet(Node, "class", true);
            bool ClassAdded = false;
            if (!string.IsNullOrEmpty(NewClass))
            {
                ClassAdded = true;
                info.ActiveClasses.Add(NewClass);
            }
            switch (Node.Name)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    info.ActiveClasses.Add(Node.Name);
                    foreach (var c in Node.ChildNodes) this.AddAppropriatePart(c, info);
                    Parts.Add(new BreakLinePart(info));
                    info.ActiveClasses.Remove(Node.Name);
                    break;
                case "#text":
                case "nav":
                    Text = Node.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(Text)) Parts.Add(new TextLinePart(Text.Trim(), info));
                    break;
                case "ruby":
                    if (Node.ChildNodes.Count >= 2)
                    {
                        foreach (var Child in Node.ChildNodes)
                        {
                            /*
                             * <ruby>
                                 漢 <rp>(</rp><rt>Kan</rt><rp>)</rp> 字 <rp>(</rp><rt>ji</rt><rp>)</rp>
                                </ruby>
                             */
                            bool bracketIncoming = false;
                            if (Child.Name == "rp")
                            {
                                bracketIncoming = true;
                                continue;
                            }
                            if (bracketIncoming)
                            {
                                bracketIncoming = false;
                                continue;
                            }
                            info.Splittable = false;
                            if (Child.Name == "rt") info.IsRuby = true;
                            else info.Splittable = false;
                            this.AddAppropriatePart(Child, info);
                            if (Child.Name == "rt") info.IsRuby = false;
                            info.Splittable = true;
                        }
                    }
                    else
                    {
                        Logger.Report("Broken ruby found, ignoring", LogType.Error);
                    }
                    break;
                case "hr":
                case "br":
                    Parts.Add(new BreakLinePart(info));
                    break;
                case "span":
                    AddSpanElement(Node, info);
                    break;
                case "a":
                case "svg":
                case "div":
                    AddChapterMarker(Node, info);
                    foreach (var ChildNode in Node.ChildNodes) AddAppropriatePart(ChildNode, info);
                    break;
                case "p":
                    AddChapterMarker(Node, info);
                    if (Node.ChildNodes.Count > 1 || Node.FirstChild.Name != "br")
                    {
                        foreach (var ChildNode in Node.ChildNodes) AddAppropriatePart(ChildNode, info);
                    }
                    Parts.Add(new BreakLinePart(info));
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
                    var Image = new ImageLinePart(Link, Inline, info);
                    //Set later to allow parallelization
                    Parts.Add(Image);
                    break;
                case "ops:default":
                    Logger.Report("Switch statement found, ignoring default", LogType.Info);
                    break;
                default:
                    Logger.Report(string.Format("unknown element \"{2}\" in \"{1}\" in line \"{0}\""
                        , Node.OuterHtml, Node.ParentNode.Name, Node.Name), LogType.Error);
                    Logger.Report("trying to force parse...", LogType.Info);
                    foreach (var ChildNode in Node.ChildNodes) AddAppropriatePart(ChildNode, info);
                    break;
            }

            if (ClassAdded) info.ActiveClasses.RemoveAt(info.ActiveClasses.Count - 1);
        }

        private void AddChapterMarker(HtmlNode Node, LineSplitInfo info)
        {
            string Id = HTMLParser.SafeAttributeGet(Node, "id", true);
            if (!string.IsNullOrEmpty(Id)) Parts.Add(new ChapterMarkerLinePart(Id, info));
        }

        private void AddSpanElement(HtmlNode node, LineSplitInfo info)
        {
            AddChapterMarker(node, info);
            var classAttribute = HTMLParser.SafeAttributeGet(node, "class", true);
            var IgnoreAttribute = HTMLParser.SafeAttributeGet(node, "data-amznremoved-m8", true);
            if (IgnoreAttribute == "true") return;

            switch (classAttribute)
            {
                case "sesame":
                    if (node.ChildNodes.Count == 0) return;
                    string text = node.ChildNodes[0].InnerHtml;
                    var NewSesamePart = new TextLinePart(text, info)
                    {
                        Splittable = false
                    };
                    Parts.Add(NewSesamePart);
                    string dots = new string('﹅', text.Length);
                    Parts.Add(new TextLinePart(dots, info) { Splittable = false, IsRuby = true });
                    return;
                case "img":
                    foreach (var ChildNode in node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, info);
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
                        AddAppropriatePart(ChildNode, info);
                    }
                    return;
            }
        }
    }
}
