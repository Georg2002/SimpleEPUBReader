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
            var Page = new EpubPage(TestFile, new BookSettings());
            Assert.IsTrue(string.IsNullOrEmpty(Page.Title));
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
                            
            var Page = new EpubPage(Text, new BookSettings());

            Assert.IsTrue(Page.Title == "とある魔術の禁書目録４");
            Assert.IsTrue(Page.Language == "ja");
            Assert.IsTrue(Page.Lines.Count > 200);
            //<p>「問二。食べてみるか、というその質問から察するに、
            //これは<ruby>食物<rt>しよくもつ</rt></ruby>なのか？」</p>
            var FirstLine = Page.Lines.First();
            //<p>「手を止めなさい、火野神作。これは警告ではなく<ruby>
            //威嚇<rt>いかく</rt></ruby>です。従わねば刀を抜きます」</p>
            var LastLine = Page.Lines.Last();
            Assert.IsTrue(FirstLine.Parts.Count == 3);
            Assert.IsTrue(LinePartCorrect(FirstLine.Parts[0],
                "「問二。食べてみるか、というその質問から察するに、これは", ""));
            Assert.IsTrue(LinePartCorrect(FirstLine.Parts[1],
                "食物", "しよくもつ"));
            Assert.IsTrue(LinePartCorrect(FirstLine.Parts[2],
                "なのか？」", ""));
            Assert.IsTrue(FirstLine.Parts[0].Format == TextFormat.none);
            Assert.IsTrue(FirstLine.Parts[1].Format == TextFormat.none);

            Assert.IsTrue(LastLine.Parts.Count == 3);
            Assert.IsTrue(LastLine.Parts[0].Text ==
                "「手を止めなさい、火野神作。これは警告ではなく");

            Assert.IsTrue(Page.Lines.Exists(a => a.Parts.Exists(b => b.Format == TextFormat.sesame)));
        }

        public bool LinePartCorrect(LinePart Part, string Text, string Ruby)
        {
            return Part.Text == Text && Part.Ruby == Ruby;
        }

        [TestMethod]
        public void PageWithImageTest()
        {

            var Path = TestResources.GetTestHTMLFile(31);
            var Text = new TextFile(new ZipEntry() { Content = File.ReadAllBytes(Path) })
            {
                Name = "imagepage.xhtml"
            };
            var Page = new EpubPage(Text, new BookSettings());
            Assert.IsTrue(Page.Lines.Count == 1);
            Assert.IsTrue(Page.Lines[0].Parts.Count == 1);
            var ImageSrcPart = Page.Lines[0].Parts[0];
            Assert.IsTrue(ImageSrcPart.Format == TextFormat.image);
            Assert.IsTrue(ImageSrcPart.Ruby == "");
            Assert.IsTrue(ImageSrcPart.Text == "../images/0018.jpg");
        }
    }
}
