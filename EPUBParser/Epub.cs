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
        public tocInfo toc;

        public Epub(string FilePath)
        {
            Chapters = new List<EpubPage>();
            Package = new PackageInfo();
            toc = new tocInfo();
            if (!File.Exists(FilePath))
            {
                Name = "file missing";
                Logger.Report(string.Format("file missing: \"{0}\"", FilePath);
                return;
            }
            var Files = Unzipper.GetFiles(FilePath);
        }
    }



    public class PackageInfo
    {
    }

    public class tocInfo
    {
    }
}
