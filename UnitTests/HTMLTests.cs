using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPUBParser;
using System.IO;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class HTMLTests
    {
        [TestMethod]
        public void SimpleParseTest()
        {
            var HTMLFilePath = TestResources.GetTestHTMLFile(30);
            var Text = new TextFile( new ZipEntry() { Content = File.ReadAllBytes(HTMLFilePath) });
            var Doc = HTMLParser.Parse(Text);
            Assert.IsTrue(Doc.DocumentNode.Descendants().Count() > 10);
        }
    }
}