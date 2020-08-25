using System;
using System.Collections.Generic;
using System.Windows.Documents;
using EPUBParser;
using EPUBRenderer2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class UnitTextElementStringCreatorTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            var TestLines = new List<EpubLine>();
            var Line1 = new EpubLine();
            Line1.Parts.Add(new TextLinePart("Test1", ""));
            Line1.Parts.Add(new TextLinePart("Test2", "Test"));
            TestLines.Add(Line1);
            var Line2 = new EpubLine();
            Line2.Parts.Add(new ImageLinePart(""));
            TestLines.Add(Line2);
            var Res = TextElementStringCreator.GetElements(TestLines, true);
            Assert.IsTrue(Res.Count == 6);
            var RubyPart = Res[2];
            Assert.IsTrue(RubyPart[0].ElementType == TextElementType.RubyLetter);    
        }
    }
}
