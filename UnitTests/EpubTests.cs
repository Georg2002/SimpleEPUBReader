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
                MaxLogLength = 200
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
                MaxLogLength = 200
            };
            EpubTester(TestCase);
        }

        private void EpubTester(TestCase test)
        {
            EPUBReader.Logger.Log.Clear();
            var FilePath = test.FilePath;
            var Book = new Epub(FilePath);
            TestResources.WriteLogToFile();
            Assert.IsTrue(Book.Pages.Count == test.Pages);
            Assert.IsFalse(string.IsNullOrEmpty(Book.Pages[0].Name) );
            Assert.IsFalse(string.IsNullOrEmpty(Book.Pages[0].FullName));
            Assert.IsTrue(Book.Package.Guide.Count != 0);
            Assert.IsTrue(Book.Images.Count == test.Images);
            Assert.IsTrue(Book.Images[0].ImageData != null);
            Assert.IsTrue(Book.toc.Chapters.Count == test.Chapters);
            Assert.IsTrue(EPUBReader.Logger.Log.Count < test.MaxLogLength);
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
        public int MaxLogLength;
    }
}
