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
        public List<ImageFile> Images;
        public PackageInfo Package;
        public TocInfo toc;
        public BookSettings Settings;

        public Epub(string FilePath)
        {
            this.FilePath = FilePath;
            Pages = new List<EpubPage>();
            Images = new List<ImageFile>();
            Settings = new BookSettings();
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
                Settings.StandardRTL = Package.RightToLeft;
                Settings.StandardVertical = Package.Vertical;
                Settings.Title = Package.Title;
            }

            Package = new PackageInfo(new TextFile(PackageFile));
            if (Package.Manifest.Count > 0)
            {
                foreach (var ManifestItem in Package.Manifest)
                {
                    BaseDataFile File;
                    switch (ManifestItem.Type)
                    {
                        case MediaType.xhtml:
                            File = new TextFile(ZipEntry.GetEntryByPath(Files, ManifestItem.Path, PackageFile));
                            Pages.Add(new EpubPage((TextFile)File, Settings));
                            break;
                        case MediaType.toc:
                            File = new TextFile(ZipEntry.GetEntryByPath(Files, ManifestItem.Path, PackageFile));
                            toc = new TocInfo((TextFile)File);
                            break;
                        case MediaType.css:
                            break;
                        case MediaType.image:
                            File = new ImageFile(ZipEntry.GetEntryByPath(Files, ManifestItem.Path, PackageFile));
                            Images.Add((ImageFile)File);
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

    public class BookSettings
    {
        public bool StandardVertical = false;
        public bool StandardRTL = false;
        public string Title;
    }
}
