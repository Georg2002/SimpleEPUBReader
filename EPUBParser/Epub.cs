using EPUBReader;
using ExCSS;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public class Epub
    {
        public string Name;
        public List<EpubPage> Chapters;
        public PackageInfo Package;
        public TocInfo toc;

        public Epub(string FilePath)
        {
            Chapters = new List<EpubPage>();
            if (!File.Exists(FilePath))
            {
                Name = "file missing";
                Logger.Report(string.Format("file missing: \"{0}\"", FilePath), LogType.Error);
                return;
            }
            var Files = Unzipper.GetFiles(FilePath);


            //find files first!!
            //    Package = new PackageInfo();
            //    toc = new TocInfo();
        }
    }

    public class PackageInfo
    {
        public PackageInfo(TextFile file)
        {

        }
    }
}
