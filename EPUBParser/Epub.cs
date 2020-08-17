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
        public string FilePath;
        public List<EpubPage> Pages;        
        public PackageInfo Package;
        public TocInfo toc;
        public EpubSettings Settings;     

        public Epub(string FilePath)
        {
            this.FilePath = FilePath;
            Pages = new List<EpubPage>();
            Settings = new EpubSettings();
            Logger.Report(string.Format("parsing epub file at \"{0}\"", FilePath), LogType.Info);

            if (!File.Exists(FilePath))
            {
                Logger.Report(string.Format("file missing: \"{0}\"", FilePath), LogType.Error);
                return;
            }
            var Files = Unzipper.GetFiles(FilePath);

            var PackageFile = GetFile(Files, GlobalSettings.PackageFileNames);
            if (PackageFile == null)
            {
                Logger.Report("Package file could not be found", LogType.Error);
                Package = new PackageInfo(null);
                toc = new TocInfo(null);
            }

            Package = new PackageInfo(PackageFile);
            Settings.RTL = Package.RightToLeft;
            Settings.Vertical = Package.Vertical;
            Settings.Title = Package.Title;
            Settings.Language = Package.Language;

            if (Package.Manifest.Count > 0)
            {
                foreach (var ManifestItem in Package.Manifest)
                {
                    ZipEntry File = ZipEntry.GetEntryByPath(Files, ManifestItem.Path, PackageFile);
                    switch (ManifestItem.Type)
                    {
                        case MediaType.xhtml:
                            
                            Pages.Add(new EpubPage(File, Settings, Files));
                            break;
                        case MediaType.toc:                            
                            toc = new TocInfo(File);
                            break;
                        case MediaType.css:
                            break;
                        case MediaType.image:
                            break;
                        case MediaType.empty:
                            break;
                        case MediaType.unknown:
                            break;
                        default:
                            break;
                    }
                }
            }
            if (toc == null)
            {
                Logger.Report("toc not set", LogType.Error);
            }
        }

        private ZipEntry GetFile(List<ZipEntry> Files, string[] PossibleNames)
        {
            foreach (var Name in PossibleNames)
            {
                var Result = ZipEntry.GetEntryByName(Files, Name, true);
                if (Result == null)
                {
                    continue;
                }
                else
                {
                    return Result;
                }
            }
            return null;
        }
    }

    public class EpubSettings
    {
        public bool Vertical = false;
        public bool RTL = false;
        public string Title;
        public string Language;
    }
}
