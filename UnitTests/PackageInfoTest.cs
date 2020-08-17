using System;
using System.IO;
using EPUBParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PackageInfoTest
    {
        [TestMethod]
        public void SimplePackageInfoTest1()
        {
            var TestCase = new PackageInfoTestcase()
            {
                FilePath = Path.Combine(TestResources.TestFolderPath,
                TestResources.TestEpubExtracted1, "OPS", "package.opf"),
                Title = "とある魔術の禁書目録４",
                Creator = "鎌池和馬",
                Language = "ja",
                Vertical = true,
                RightToLeft = true,
                ManifestItems = 74,
                FirstManifestItem = new ManifestItem()
                {
                    Id = "vertical",
                    Path = "css/vertical.css",
                    Type = MediaType.css
                },
                SpineItems = 46,
                FirstSpineItem = new SpineItem()
                {
                    Id = "title-page",
                    Linear = true
                },
                GuideItems = 3,
                FirstGuideItem = new GuideItem()
                {
                    Path = "xhtml/title.xhtml",
                    Title = "title-page",
                    Type = GuideItemType.TitlePage
                }
            };
            PackageInfoTester(TestCase);
        }

        [TestMethod]
        public void SimplePackageInfoTest2()
        {
            var TestCase = new PackageInfoTestcase()
            {
                FilePath = Path.Combine(TestResources.TestFolderPath,
               TestResources.TestEpubExtracted2, "content.opf"),
                Title = "[森岡浩之] 星界の断章1",
                Creator = "森岡浩之",
                Language = "ja",
                Vertical = true,
                RightToLeft = true,
                ManifestItems = 49,
                FirstManifestItem = new ManifestItem()
                {
                    Id = "titlepage",
                    Path = "titlepage.xhtml",
                    Type = MediaType.xhtml
                },
                SpineItems = 38,
                FirstSpineItem = new SpineItem()
                {
                    Id = "titlepage",
                    Linear = true
                },
                GuideItems = 4,
                FirstGuideItem = new GuideItem()
                {
                    Path = "text/part0000.html",
                    Title = "扉",
                    Type = GuideItemType.TitlePage
                }
            };
            PackageInfoTester(TestCase);
        }

        private void PackageInfoTester(PackageInfoTestcase TestCase)
        {
            var TextFile = new ZipEntry()
            {
                Content = File.ReadAllBytes(TestCase.FilePath),
                Name = "TestTextFile"
            };
            var Info = new PackageInfo(TextFile);
            Assert.IsTrue(Info.Title == TestCase.Title);
            Assert.IsTrue(Info.Creator == TestCase.Creator);
            Assert.IsTrue(Info.Language == TestCase.Language);
            Assert.IsTrue(Info.Title == TestCase.Title);
            Assert.IsTrue(Info.Vertical == TestCase.Vertical);
            Assert.IsTrue(Info.RightToLeft == TestCase.RightToLeft);
            Assert.IsTrue(Info.Manifest.Count == TestCase.ManifestItems);
            Assert.IsTrue(Info.Manifest[0].Equals(TestCase.FirstManifestItem));
            Assert.IsTrue(Info.Spine.Count == TestCase.SpineItems);
            Assert.IsTrue(Info.Spine[0].Equals(TestCase.FirstSpineItem));
            Assert.IsTrue(Info.Guide.Count == TestCase.GuideItems);
            Assert.IsTrue(Info.Guide[0].Equals(TestCase.FirstGuideItem));
        }

        private class PackageInfoTestcase
        {
            public string FilePath;
            public string Title;
            public string Creator;
            public string Language;
            public bool Vertical;
            public bool RightToLeft;
            public int ManifestItems;
            public ManifestItem FirstManifestItem;
            public int SpineItems;
            public SpineItem FirstSpineItem;
            public int GuideItems;
            public GuideItem FirstGuideItem;
        }
    }
}
