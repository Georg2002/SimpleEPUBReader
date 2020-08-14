using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPUBParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class EpubTests
    {
        [TestMethod]
        public void SimpleEpubTest1()
        {
            var TestCase = new TestCase()
            {
                FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub1),
                Pages = 46,
                Images = 22,
                Chapters = 15,
                MaxLogLength = 200,
                GuidCount = 3,
                Title = "とある魔術の禁書目録４",
                RTL = true,
                Vertical = true
            };
            EpubTester(TestCase);
        }

        [TestMethod]
        public void SimpleEpubTest2()
        {
            var TestCase = new TestCase()
            {
                FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub2),
                Pages = 38,
                Images = 8,
                Chapters = 14,
                MaxLogLength = 200,
                GuidCount = 4,
                Title = "[森岡浩之] 星界の断章1",
                 RTL = true,
                Vertical = true
            };
            EpubTester(TestCase);
        }

        [TestMethod]
        public void SimpleEpubTest3()
        {
            var TestCase = new TestCase()
            {
                FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub3),
                Pages = 4,
                Images = 0,
                Chapters = 9,
                MaxLogLength = 200,
                Title = "ダンジョンに出会いを求めるのは間違っているだろうか外伝",
                RTL = true,
                Vertical = true
            };
            EpubTester(TestCase);
        }

        private void EpubTester(TestCase test)
        {
            Logger.Log.Clear();
            var FilePath = test.FilePath;
            var Book = new Epub(FilePath);
            TestResources.WriteLogToFile();
            Assert.IsTrue(Book.Pages.Count == test.Pages);
            Assert.IsFalse(string.IsNullOrEmpty(Book.Pages[0].Name));
            Assert.IsFalse(string.IsNullOrEmpty(Book.Pages[0].FullName));
            Assert.IsTrue(Book.Package.Guide.Count == test.GuidCount);
            Assert.IsTrue(Book.Images.Count == test.Images);
            Assert.IsTrue(Book.Settings.Title == test.Title);
            foreach (var Image in Book.Images)
            {
                Assert.IsTrue(Image.ImageData != null);
            }
            Assert.IsTrue(Book.toc.Chapters.Count == test.Chapters);
            Assert.IsTrue(Logger.Log.Count < test.MaxLogLength);
        }
    }


    public class TestCase
    {
        public string FilePath;
        //xhtml files, including title, navigation, and everything else in the folder
        public int Pages;
        public string Title;
        public int Images;
        //as written in the toc.ncx file
        public int Chapters;
        public int GuidCount;
        public int MaxLogLength;
        public bool RTL;
        public bool Vertical;
    }
}
