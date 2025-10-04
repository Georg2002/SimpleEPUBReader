using ExCSS;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public class EpubPage : IBaseFile
    {
        public EpubSettings PageSettings;
        public List<EpubLine> Lines;


        public string Name { get; set; }
        public string FullName { get; set; }
        public override string ToString() => PageSettings.Title;
        private List<ZipEntry> Entries;
        private EpubSettings BaseSettings;
        private ZipEntry File;
        public bool Initialized { get; private set; } = false;
        public void Init()
        {
            if (this.Initialized) return;
            this.Initialized = true;

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
                PageSettings.Language = BaseSettings.Language;
                PageSettings.Vertical = BaseSettings.Vertical;
                PageSettings.RTL = BaseSettings.RTL;
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
                if (string.IsNullOrEmpty(ParsedTitle))
                {
                    Logger.Report("title not found, set to standard", LogType.Info);
                    PageSettings.Title = BaseSettings.Title;
                }
                else PageSettings.Title = ParsedTitle;
            }

            var BodyNode = HTMLParser.SafeNodeGet(htmlNode, "body");
            if (BodyNode != null)
            {
                foreach (var Node in BodyNode.ChildNodes)
                {
                    if (Node.Name != "#text")
                    {
                        var NewLine = new EpubLine(Node, Entries, File);
                        if (NewLine.Parts.Count > 0) Lines.Add(NewLine);
                    }
                }
            }
            //to make getting images from web faster
            Lines.AsParallel().ForAll(a => a.Parts.Where(b => b.Type == LinePartTypes.image)
            .AsParallel().ForAll(c => ((ImageLinePart)c).SetImage(Entries, File)));


            //clear reference to allow gc to collect
            this.File = null;
            this.BaseSettings = null;
            this.Entries = null;
        }
        public void FreeMemory()
        {
            //delete references for unused large objects
            this.Lines = null;
            this.Entries = null;
        }
        public EpubPage(ZipEntry File, EpubSettings Settings, List<ZipEntry> Entries)
        {
            PageSettings = new EpubSettings();
            Lines = new List<EpubLine>();
            this.Name = File.Name;
            this.FullName = File.FullName;
            this.File = File;
            this.BaseSettings = Settings;
            this.Entries = Entries;
        }
    }
}
