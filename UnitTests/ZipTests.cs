using System;
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
            ZipEntry SubFolder = Files.FirstOrDefault(a => a.Name == "Testsubfolder");
            Assert.IsTrue(SubFolder != null);
            Assert.IsTrue(SubFolder.Subentries.Count == 1);
            Assert.IsTrue(SubFolder.EntryType == ZipEntryType.Folder);
            Assert.IsTrue(SubFolder.Subentries[0].EntryType == ZipEntryType.File);
            Assert.IsTrue(SubFolder.Subentries[0].Subentries.Count == 0);
            ZipEntry File1 = Files.First(a => a.EntryType == ZipEntryType.File);
            Assert.IsTrue(File1.Content.Length != 0);
        }

        [TestMethod]
        public void FileSearchTest()
        {
            var Files = new List<ZipEntry>();
            Files.Add(new ZipEntry() { Name = "1", FullName = "1" });
            Files[0].Subentries.Add(new ZipEntry() { Name = "2", FullName = "1/2" });
            Files[0].Subentries.Add(new ZipEntry() { Name = "3", FullName = "1/3" });
            Files[0].Subentries[1].Subentries.Add(new ZipEntry() { Name = "4", FullName = "1/3/4" });
            var Success = ZipEntry.GetEntry(Files, "1/3/4");
            var Fail = ZipEntry.GetEntry(Files, "1/3/6");
            Assert.IsNotNull(Success);
            Assert.IsNull(Fail);
        }

        [TestMethod]
        public void TextFileTest()
        {
            string TestText = "Testてすと\nLine2";
            var ByteData = Encoding.UTF8.GetBytes(TestText);
            var File = new TextFile(ByteData);       
            Assert.IsTrue(TestText == File.Text);          
        }

        [TestMethod]
        public void ImageFileTest()
        {
            string TestImagePath = Path.Combine(TestResources.TestFolderPath, TestResources.TestImageName);
            var ByteData = File.ReadAllBytes(TestImagePath);
            ImageFile ImageFile = new ImageFile(ByteData);
            var Image = ImageFile.GetImage();
            Assert.IsTrue(Image.Width == 100 && Image.Height == 100);
        }
    }
}
