﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPUBParser;
using EPUBReader;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class ZipTests
    {
        [TestMethod]
        public void SimpleUnzippingTest()
        {
            var FilePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestZipName);
            var Files = Unzipper.GetFiles(FilePath);
            Assert.IsTrue(Files.Count == 2);
        }

        [TestMethod]
        public void EpubUnzippingTest()
        {
            var Files = Unzipper.GetFiles(Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub1));
            Assert.IsTrue(Files.Count == 78);
            Assert.IsNotNull(ZipEntry.GetEntryByName(Files, "package.opf"));
            Assert.IsNotNull(ZipEntry.GetEntryByPath(Files, "OPS/images/0002.jpg"));
        }

        [TestMethod]
        public void FilePathSearchTest()
        {
            var Files = new List<ZipEntry>
            {
                new ZipEntry() { Name = "1", FullName = "1" }
            };
            Files.Add(new ZipEntry() { Name = "2", FullName = "1/2" });
            Files.Add(new ZipEntry() { Name = "3", FullName = "1/3" });
            Files.Add(new ZipEntry() { Name = "4", FullName = "1/3/4" });
            var Success = ZipEntry.GetEntryByPath(Files, "1/3/4");
            var Fail = ZipEntry.GetEntryByPath(Files, "1/3/2");
            Assert.IsNotNull(Success);
            Assert.IsNull(Fail);
        }

        [TestMethod]
        public void FileRelativePathSearchTest()
        {
            //1
            //F1
            //  3
            //  F2
            //    4
            var Files = new List<ZipEntry>
            {
                new ZipEntry() { Name = "1", FullName = "1" },
                new ZipEntry() { Name = "2", FullName = "F1/2" },
                new ZipEntry() { Name = "3", FullName = "F1/3" },
                new ZipEntry() { Name = "4", FullName = "F1/F2/4" }
            };    
            var SuccessNormal = ZipEntry.GetEntryByPath(Files, "F1/F2/4", Files[0]);
            var SuccesRelative1 = ZipEntry.GetEntryByPath(Files, "F2/4", Files[1]);
            var SuccesRelative2 = ZipEntry.GetEntryByPath(Files, "../F2/4", Files[3]);
            var Fail = ZipEntry.GetEntryByPath(Files, "F1/F2/2",Files[0]);
            Assert.IsTrue(Files[3] == SuccessNormal);
            Assert.IsTrue(Files[3] == SuccesRelative1);
            Assert.IsTrue(Files[3] == SuccesRelative2);
            Assert.IsNull(Fail);
        }

        [TestMethod]
        public void FileNameSearchTest()
        {
            var Files = Unzipper.GetFiles(Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub1));
            var tocFile = ZipEntry.GetEntryByName(Files, "toc.ncx");
            Assert.IsNotNull(tocFile);
            Assert.IsTrue(tocFile.Name == "toc.ncx");
        }
    }
}
