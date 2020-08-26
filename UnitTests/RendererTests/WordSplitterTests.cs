using System;
using System.IO;
using EPUBParser;
using EPUBRenderer  ;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class WordSplitterTests
    {
        [TestMethod]
        public void SimpleSplitterTest()
        {
            var TestPage = new EpubPage(new ZipEntry(), null, null);
            var Line1 = new EpubLine();
            Line1.Parts.Add(new TextLinePart("the test,it starts here", ""));
            Line1.Parts.Add(new TextLinePart("the test, this is the second one", "the next, part"));

            var Line2 = new EpubLine();
            Line2.Parts.Add(new TextLinePart("dod, odu", "a"));
            TestPage.Lines.Add(Line1);
            TestPage.Lines.Add(Line2);
            var Res = WordSplitter.SplitIntoWords(TestPage);
            Assert.IsTrue(Res.Count == 2);
            var L1 = Res[0];
            var L2 = Res[1];
            Assert.IsTrue(L1.Parts.Count == 6);
            Assert.IsTrue(L1.Parts[0].Text == "the ");
            Assert.IsTrue(L2.Parts.Count == 1);
        }
    }
}
