using System;
using EPUBRenderer;
using EPUBParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Documents;
using System.Windows;
using System.Collections.Generic;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class RubyWritingTests
    {
        [TestMethod]
        public void OneMainOneRubyTest()
        {
            FlowDirectionModifiers.Direction = WritingFlow.VRTL;
            ChapterPagesCreator.FontSize = 25;
            ChapterPagesCreator.RubyFontSize = 0.1;
            var Page = new PageRenderer(new EpubSettings(), new Vector(1000, 1000));
            var TestWritings = new List<Writing>() { new Writing() {FontSize=25, WritingPosition = new Vector(500, 125) } };
          var Res =  Page.GetRubyWritings("あ", TestWritings);
          //  Assert.IsTrue(Res[0].WritingPosition.Y == 188.75);
            ChapterPagesCreator.RubyFontSize = 1;
            Res = Page.GetRubyWritings("あ", TestWritings);
            Assert.IsTrue(Res[0].WritingPosition.Y == 125);
        }

        [TestMethod]
        public void TwoMainOneRubyTest()
        {
            FlowDirectionModifiers.Direction = WritingFlow.VRTL;
            ChapterPagesCreator.FontSize = 25;
            ChapterPagesCreator.RubyFontSize = 0.1;
            var Page = new PageRenderer(new EpubSettings(), new Vector(1000, 1000));
            var TestWritings = new List<Writing>() { new Writing() { FontSize = 25, WritingPosition = new Vector(500, 200) },
             new Writing() { FontSize = 25, WritingPosition = new Vector(500, 225) } };
            var Res = Page.GetRubyWritings("あ", TestWritings);
            Assert.IsTrue(Res[0].WritingPosition.Y == 201.25);
            ChapterPagesCreator.RubyFontSize = 1;
            Res = Page.GetRubyWritings("あ", TestWritings);
            Assert.IsTrue(Res[0].WritingPosition.Y == 212.5);
        }

        [TestMethod]
        public void OneMainTwoRubyTest()
        {
            FlowDirectionModifiers.Direction = WritingFlow.VRTL;
            ChapterPagesCreator.FontSize = 25;
            ChapterPagesCreator.RubyFontSize = 0.1;
            var Page = new PageRenderer(new EpubSettings(), new Vector(1000, 1000));
            var TestWritings = new List<Writing>() { new Writing() { FontSize = 25, WritingPosition = new Vector(500, 200) }};
            var Res = Page.GetRubyWritings("ああ", TestWritings);
            Assert.IsTrue(Res[0].WritingPosition.Y == 176.25);
            Assert.IsTrue(Res[1].WritingPosition.Y == 201.25);
            ChapterPagesCreator.RubyFontSize = 1;
            Res = Page.GetRubyWritings("ああ", TestWritings);
            Assert.IsTrue(Res[0].WritingPosition.Y == 187.5);
            Assert.IsTrue(Res[1].WritingPosition.Y == 212.5);
        }
    }
}
