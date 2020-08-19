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
        private WritingDirection WritingDirection;

        [TestInitialize]
        public void Init()
        {
            renderer = TestResources.CommonInit();
            WritingDirection = renderer.WritingDirection;
        }

        [TestMethod]
        public void VRTLPageFullTest()
        {
            var Testcase = new PageFullTestcase()
            {
                NotFullPos = new Point(20, 0),
                FullPos = new Point(19, 0),
                Direction = WritingFlow.VRTL
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
                Direction = WritingFlow.VLTR
            };
            PageFullTester(Testcase);
        }

        private void PageFullTester(PageFullTestcase Testcase)
        {
            renderer.Direction = Testcase.Direction;
            renderer.CurrentWritePosition = Testcase.NotFullPos;
            Assert.IsTrue(!WritingDirection.PageFull());
            renderer.CurrentWritePosition = Testcase.FullPos;
            WritingDirection.renderer = renderer;
            Assert.IsTrue(WritingDirection.PageFull());
        }

        private class PageFullTestcase
        {
            public Point NotFullPos;
            public Point FullPos;      
            public WritingFlow Direction;
        }
    }
}
