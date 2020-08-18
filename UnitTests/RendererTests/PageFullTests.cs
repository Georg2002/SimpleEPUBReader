using System;
using System.Windows;
using System.Windows.Controls;
using EPUBRenderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class PageFullTests
    {
        private PageRenderer renderer;

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
        public void VRTLPageFullTest()
        {
            var Testcase = new PageFullTestcase()
            {
                NotFullPos = new Point(20, 0),
                FullPos = new Point(19, 0),
                Direction = PageRenderer.WritingFlow.VRTL
            };
            PageFullTester(Testcase);
        }

        [TestMethod]
        public void VLTRPageFullTest()
        {
            var Testcase = new PageFullTestcase()
            {
                NotFullPos = new Point(480, 0),
                FullPos = new Point(481, 0),
                Direction = PageRenderer.WritingFlow.VLTR
            };
            PageFullTester(Testcase);
        }

        private void PageFullTester(PageFullTestcase Testcase)
        {
            renderer.Direction = Testcase.Direction;
            renderer.CurrentWritePosition = Testcase.NotFullPos;
            Assert.IsTrue(!renderer.PageFull());
            renderer.CurrentWritePosition = Testcase.FullPos;
            Assert.IsTrue(renderer.PageFull());
        }

        private class PageFullTestcase
        {
            public Point NotFullPos;
            public Point FullPos;      
            public PageRenderer.WritingFlow Direction;
        }
    }
}
