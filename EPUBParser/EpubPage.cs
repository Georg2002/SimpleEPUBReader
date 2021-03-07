using ExCSS;
using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EPUBParser
{
    public class EpubPage : IBaseFile
    {
        public EpubSettings PageSettings;
        public List<EpubLine> Lines;

        public string Name { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return PageSettings.Title;
        }

        public EpubPage(ZipEntry File, EpubSettings Settings, List<ZipEntry> Entries)
        {
            PageSettings = new EpubSettings();
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
                PageSettings.Language = Settings.Language;
                PageSettings.Vertical = Settings.Vertical;
                PageSettings.RTL = Settings.RTL;
            }
            else
            {
                PageSettings.Language = LangAttr;
                PageSettings.Vertical = GlobalSettings.IsVerticalLanguage(LangAttr);
                PageSettings.RTL = GlobalSettings.IsRtLLanguage(LangAttr);
            }

            var HeadNode = HTMLParser.SafeNodeGet(htmlNode, "head");
            if (HeadNode != null)
            {
                var ParsedTitle = HTMLParser.SafeNodeTextGet(HeadNode, "title");
                if (ParsedTitle == "")
                {
                    Logger.Report("title not found, set to standard", LogType.Info);
                    PageSettings.Title = Settings.Title;
                }
                else
                {
                    PageSettings.Title = ParsedTitle;
                }
            }

            var BodyNode = HTMLParser.SafeNodeGet(htmlNode, "body");
            if (BodyNode != null)
            {
                foreach (var Node in BodyNode.ChildNodes)
                {
                    if (Node.Name != "#text")
                    {
                        var NewLine = new EpubLine(Node, Entries, File);
                        if (NewLine.Parts.Count > 0)
                        {
                            Lines.Add(NewLine);
                        }
                    }
                }
            }
            //to make getting images from web faster
            Lines.AsParallel().ForAll(a => a.Parts.Where(b => b.Type == LinePartTypes.image)
            .AsParallel().ForAll(c => ((ImageLinePart)c).SetImage(Entries, File)));
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
        public EpubLine(HtmlNode HtmlLine, List<ZipEntry> Entries, ZipEntry File)
        {
            Parts = new List<LinePart>();
            if (HtmlLine == null)
            {
                Logger.Report("node has value null", LogType.Error);
                return;
            }
            AddAppropriatePart(HtmlLine, Entries, File);
        }

        private void AddAppropriatePart(HtmlNode Node, List<ZipEntry> Entries, ZipEntry File)
        {
            string Text = "";
            switch (Node.Name)
            {
                case "#text":
                case "nav":                    
                    Text = Node.InnerText;
                   if (!string.IsNullOrWhiteSpace(Text))
                    {                  
                        Parts.Add(new TextLinePart(Text, ""));
                    }                  
                    break;
                case "ruby":
                    if (Node.ChildNodes.Count >= 2)
                    {
                        string Ruby = "";
                        foreach (var Child in Node.ChildNodes)
                        {
                            if (Child.Name == "rt")
                            {
                                Ruby += Child.InnerText;
                            }
                            else if (Child.Name=="#text" || Child.Name=="rb")
                            {
                                Text += Child.InnerText;
                            }
                            else
                            {
                                Logger.Report("Broken ruby found, ignoring", LogType.Error);
                                break;
                            }
                        }                       
                        Parts.Add(new TextLinePart(Text, Ruby));
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
                    AddSpanElement(Node, Entries, File);
                    break;
                case "a":
                case "svg":
                case "div":
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, Entries, File);
                    }            
                    break;
                case "p":
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, Entries, File);
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
                    var Image = new ImageLinePart(Link);
                    //Set later to allow parallelization
                    Parts.Add(Image);              
                    break;
                default:
                    Logger.Report(string.Format("unknown element \"{2}\" in \"{1}\" in line \"{0}\""
                        , Node.OuterHtml, Node.ParentNode.Name, Node.Name), LogType.Error);
                    Logger.Report("trying to force parse...", LogType.Info);
                    foreach (var ChildNode in Node.ChildNodes)
                    {
                        AddAppropriatePart(ChildNode, Entries, File);
                    }
                    break;
            }
        }

        private void AddSpanElement(HtmlNode node, List<ZipEntry> Entries, ZipEntry File)
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
                        AddAppropriatePart(ChildNode, Entries, File);
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
                        AddAppropriatePart(ChildNode, Entries, File);
                    }
                    return;
            }
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
            Text = "";
            Ruby = "";
        }
    }

    public class ImageLinePart : LinePart
    {
        private byte[] ImageData;

        public ImageSource GetImage()
        {
            if (ImageData == null)
            {
                Logger.Report(string.Format("image at \"{0}\" missing", Text), LogType.Error);
                return null;
            }
            BitmapImage Image = null;
            try
            {
                Image = new BitmapImage();
                using (var mem = new MemoryStream(ImageData))
                {
                    mem.Position = 0;
                    Image.BeginInit();
                    Image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    Image.CacheOption = BitmapCacheOption.OnLoad;
                    Image.UriSource = null;
                    Image.StreamSource = mem;
                    Image.EndInit();
                }
                Image.Freeze();
            }
            catch (Exception ex)
            {
                Logger.Report(string.Format("image from \"{0}\" couldn't be loaded", Text), LogType.Error);
                Logger.Report(ex.Message, LogType.Error);
            }

            return Image;
        }

        public ImageLinePart(string Path)
        {
            this.Text = Path;
            this.Type = LinePartTypes.image;
        }

        public void SetImage(List<ZipEntry> Entries, ZipEntry PageEntry)
        {
            ImageData = null;
            if (Text.StartsWith("http"))
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        ImageData = client.DownloadData(Text);
                    }
                }
                catch (Exception)
                {
                    Logger.Report(string.Format("failed to download image from \"{0}\"", Text), LogType.Error);
                }
            }
            else
            {
                ZipEntry Entry = ZipEntry.GetEntryByPath(Entries, Text, PageEntry);
                if (Entry != null)
                {
                    ImageData = Entry.Content;
                }
            }
            if (ImageData == null)
            {
                Logger.Report(string.Format("Image from \"{0}\" not found", Text), LogType.Error);
                return;
            }
        }
    }

    public class BreakLinePart : LinePart
    {
        public BreakLinePart()
        {
            Type = LinePartTypes.paragraph;
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
        normal, sesame, image, paragraph
    }
}
