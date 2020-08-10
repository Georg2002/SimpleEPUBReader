using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public static class TestResources
    {
        public const string TestFolderPath = @"..\..\..\TestResources";
        public const string TestZipName = @"Test.zip";
        public const string TestImageName = @"TestPicture.png";
        public const string TestEpub1 = @"Index4.epub";
        public const string TestEpub2 = @"星界の紋章第一巻.epub";
        public const string TestEpubExtracted = @"Index4";
        public static string TestHTMLFolder
        {
            get
            {
                return Path.Combine(TestFolderPath, TestEpubExtracted, "OPS","xhtml");
            }
        }
        public static string GetTestHTMLFile(int FileNumber)
        {
            return Path.Combine(TestHTMLFolder, FileNumber.ToString("D4") + ".xhtml");
        }
    }
}
