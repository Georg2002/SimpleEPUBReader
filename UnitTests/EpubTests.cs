﻿using System;
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
                Chapters = 15
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
                Chapters = 14
            };
            EpubTester(TestCase);
        }

        private void EpubTester(TestCase test)
        {
            var FilePath = test.FilePath;
            var Book = new Epub(FilePath);           
            Assert.IsTrue(Book.Pages.Count == test.Pages);
            Assert.IsTrue(Book.Package.Guide.Count != 0);
            Assert.IsTrue(Book.Images.Count == test.Images);
            Assert.IsTrue(Book.Images[0].ImageData != null);
            Assert.IsTrue(Book.toc.Chapters.Count == test.Chapters);
            TestResources.WriteLogToFile();
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
    }
}
