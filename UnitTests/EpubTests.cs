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

            ////////////left off!!!!!
            var File = Path.Combine(TestResources.TestFolderPath, TestResources.TestEpub1);
        }
    }


    public class TestCase
    {
        public string FilePath;
        //xhtml files
        public int Pages;
        public string Title;
        public int Images;
        //as written in the toc.ncx file
        public int Chapters;
    }
}
