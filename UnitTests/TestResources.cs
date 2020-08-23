using EPUBRenderer;
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
        public const string TestEpub3 = @"DanMachi.epub";
        public const string TestEpubExtracted1 = @"Index4";
        public const string TestEpubExtracted2 = @"星界の紋章第一巻";
        public const string TestEpubExtracted3 = @"DanMachi";

        public static string TestHTMLFolder1
        {
            get
            {
                return Path.Combine(TestFolderPath, TestEpubExtracted1, "OPS","xhtml");
            }
        }
        public static string GetTestHTMLFile(int FileNumber)
        {
            return Path.Combine(TestHTMLFolder1, FileNumber.ToString("D4") + ".xhtml");
        }

        public static void WriteLogToFile()
        {
            File.WriteAllLines(@"..\..\..\TestLog.txt", EPUBParser.Logger.Log);
        }
    }
}
