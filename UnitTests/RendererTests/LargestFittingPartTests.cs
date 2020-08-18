using System;
using System.Windows;
using EPUBParser;
using EPUBRenderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class LargestFittingPartTests
    {
        PageRenderer renderer;

        [TestInitialize]
        public void Init()
        {
            PageRenderer.FontSize = 10;
            PageRenderer.LineSpace = 2;

            renderer = new PageRenderer
            {
                PageHeight = 1000,
                PageWidth = 500,
            };
        }

        [TestMethod]
        public void RubySesameTest()
        {
            renderer.CurrentWritePosition = new Point(480, 961);

            renderer.Direction = PageRenderer.WritingFlow.VRTL;

            var Part = new TextLinePart("Test", "Test");

            var Res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(Res.Ruby == "");
            Assert.IsTrue(Res.Text == "");

            Part = new TextLinePart("Test", "");
            Part.Type = LinePartTypes.sesame;

            Res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(Res.Ruby == "");
            Assert.IsTrue(Res.Text == "");

            renderer.CurrentWritePosition = new Point(480, 0);

            Part = new TextLinePart("Test", "Test");

            Res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(Res.Ruby == "Test");
            Assert.IsTrue(Res.Text == "Test");

            Part = new TextLinePart("Test", "");
            Part.Type = LinePartTypes.sesame;

            Res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(Res.Ruby == "");
            Assert.IsTrue(Res.Text == "Test");
            Assert.IsTrue(Res.Type == LinePartTypes.sesame);
        }

        [TestMethod]
        public void SimpleCutOffTest()
        {
            var Part = new TextLinePart("Test", "");
            renderer.CurrentWritePosition = new Point(480, 980);
            var res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(res.Ruby == "");
            Assert.IsTrue(res.Text == "Te");
        }

        [TestMethod]
        public void SimpleFullTest()
        {
            var Part = new TextLinePart("Test", "");
            renderer.CurrentWritePosition = new Point(480, 960);
            var res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(res.Ruby == "");
            Assert.IsTrue(res.Text == "Test");
        }

        [TestMethod]
        public void SimpleNoRoomTest()
        {
            var Part = new TextLinePart("Test", "");
            renderer.CurrentWritePosition = new Point(480, 995);
            var res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(res.Ruby == "");
            Assert.IsTrue(res.Text == "");
        }

        [TestMethod]
        public void SimpleLongTextTest()
        {
            renderer.CurrentWritePosition = new Point(480, 0);
            var Part = new TextLinePart(
                @"　あるいは計画が成功しても、壮挙という評価には値しない、
という意見もきこえます。たかだか一星系の可住化事業に過ぎないこの計画など人
類社会全体から見れば、ごくささやかなものである、と。たしかにそのとおりです。
人類の歴史にこの計画が与える影響はごく少ないものでしょう。全人類がこの計画
に拍手は期待できません。また、いささか後ろ暗い部分が含まれているのも事実。
ですが、それがどうしたというのでしょう。先祖たちは人類社会全体の潮流に背を
向けて生きていくことを、つまりは隠者であることを選択し、この都市を建設しま
した。隠者たるわれわれがなぜ他者の評価を欲するのか理解できません。", "");
           var  res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(res.Text.Length == 100);

        }

        [TestMethod]
        public void NotFromZeroTest()
        {
            var Part = new TextLinePart("TestTest", "");
            renderer.CurrentWritePosition = new Point(480, 980);
            var Pos = renderer.CurrentPos;
            Pos.CharIndex = 2;
            renderer.CurrentPos = Pos;
            var res = renderer.GetLargestFittingLinePart(Part);
            Assert.IsTrue(res.Text.Length == 2);
            Assert.IsTrue(renderer.CurrentPos.CharIndex == 4);
        }

        [TestMethod]
        public void QuickFindTests()
        {
            QuickFindTester(0, 10, 0, 10);

            QuickFindTester(10, 20, 0, 10000);
        }

        private void QuickFindTester(int Aim, int Attempts, int charIndex, int Length)
        {
            double res = 0;
            bool LastFit = false;
            int Needed =-1;
            for (int i = 0; i < Attempts; i++)
            {
                if (i == 0)
                {
                    res = renderer.GetQuickFindPos(i,charIndex, Length, false, -1);
                }
                else
                {
                    res = renderer.GetQuickFindPos(i,charIndex, Length, LastFit, res);
                }
                LastFit = res <= Aim;
                if (Needed == -1 && Math.Round(res) == Aim)
                {
                    Needed = i;
                }
            }

            Assert.IsTrue(Aim == Math.Round(res));
        }
    }
}
