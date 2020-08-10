using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class EpubTests
    {
        [TestMethod]
        public void SimpleEpubTest1()
        {
            //   var TestCase = new TestCase()
            //   {
            //       FilePath = Path.Combine(TestResources.TestFolderPath,TestResources.TestEpub1),
            //       Pages = 46,
            //       Title = "とある魔術の禁書目録４",
            //       Images = 22,
            //       Chapters = 
            //   }
            Assert.Fail("not implemented, implement the actual parts first!");

        }

        private void EpubTester(TestCase test)
        {
            var File = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub1);

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
