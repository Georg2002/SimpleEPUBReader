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
    public class EpubPageTests
    {
        [TestMethod]
        public void BrokenDocumentTest()
        {
            var TestFile = new TextFile(new ZipEntry() {Name= "FailedTestFile" });        
            //tests fails if throws, so no assert needed
            var Page = new EpubPage(TestFile, new EpubSettings());
            Assert.IsTrue(string.IsNullOrEmpty(Page.PageSettings.Title));
            Assert.IsTrue(Page.Lines.Count == 0);
        }

        [TestMethod]
        public void SimpleTest()
        {
            var Path = TestResources.GetTestHTMLFile(30);
            var Text = new TextFile(new ZipEntry()
            {
                Content = File.ReadAllBytes(Path),
                Name = "Testpage1.xhtml"
            });
                            
            var Page = new EpubPage(Text, new EpubSettings());

            Assert.IsTrue(Page.PageSettings.Title == "とある魔術の禁書目録４");
            Assert.IsTrue(Page.PageSettings.Language == "ja");
            Assert.IsTrue(Page.PageSettings.RTL == true);
            Assert.IsTrue(Page.PageSettings.Vertical == true);
            Assert.IsTrue(Page.Lines.Count > 200);
            //<p>「問二。食べてみるか、というその質問から察するに、
            //これは<ruby>食物<rt>しよくもつ</rt></ruby>なのか？」</p>
            var FirstLine = Page.Lines.First();
            //<p>「手を止めなさい、火野神作。これは警告ではなく<ruby>
            //威嚇<rt>いかく</rt></ruby>です。従わねば刀を抜きます」</p>
            var LastLine = Page.Lines.Last();          
            Assert.IsTrue(TextLinePartCorrect(FirstLine.Parts[0],
                "「問二。食べ", ""));
            Assert.IsTrue(TextLinePartCorrect(FirstLine.Parts[4],
                "食物", "しよくもつ"));
            Assert.IsTrue(TextLinePartCorrect(FirstLine.Parts[5],
                "なのか？」", ""));
            Assert.IsTrue(FirstLine.Parts[0].Type == LinePartTypes.normal);
            Assert.IsTrue(FirstLine.Parts[1].Type == LinePartTypes.normal);
         
            Assert.IsTrue(LastLine.Parts[0].Text ==
                "「手を止めな");

            Assert.IsTrue(Page.Lines.Exists(a => a.Parts.Exists(b => b.Type == LinePartTypes.sesame)));
        }

        public bool TextLinePartCorrect(LinePart Part, string Text, string Ruby)
        {
            var TextPart = (TextLinePart)Part;
            return TextPart.Text == Text && TextPart.Ruby == Ruby;
        }

        [TestMethod]
        public void PageWithImageTest()
        {
            var Path = TestResources.GetTestHTMLFile(31);
            var Text = new TextFile(new ZipEntry() { Content = File.ReadAllBytes(Path) })
            {
                Name = "imagepage.xhtml"
            };
            var Page = new EpubPage(Text, new EpubSettings());
            Assert.IsTrue(Page.Lines.Count == 1);
            Assert.IsTrue(Page.Lines[0].Parts.Count == 1);
            var ImageSrcPart = Page.Lines[0].Parts[0];
            Assert.IsTrue(ImageSrcPart.Type == LinePartTypes.image);         
            Assert.IsTrue(ImageSrcPart.Text == "../images/0018.jpg");
        }
    }
}
