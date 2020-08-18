using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using EPUBReader;
using EPUBRenderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class WrapNeededTests
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
        public void VRTLWrapTest()
        {
            var Testcase = new WrapTestcase()
            {
                NoWrapPos = new Point(480, 0),
                Direction = PageRenderer.WritingFlow.VRTL,
                SingleCharWrapPos = new Point(480, 991),
                SingleCharNearlyWrapPos = new Point(480, 990),
                TwoCharWrapPos = new Point(480, 981)
            };
            WrapTest(Testcase);
        }

        [TestMethod]
        public void VLTRWrapTest()
        {
            var Testcase = new WrapTestcase()
            {
                NoWrapPos = new Point(480, 0),
                Direction = PageRenderer.WritingFlow.VRTL,
                SingleCharWrapPos = new Point(480, 991),
                SingleCharNearlyWrapPos = new Point(480, 990),
                TwoCharWrapPos = new Point(480, 981)
            };
            WrapTest(Testcase);
        }

    //    [TestMethod]
    //    public void HRTLWrapTest()
    //    {    
    //        var Testcase = new WrapTestcase()
    //        {
    //            NoWrapPos = new Point(480, 0),
    //            Direction = PageRenderer.WritingFlow.VRTL,
    //            SingleCharWrapPos = new Point(480, 991),
    //            SingleCharNearlyWrapPos = new Point(480, 990),
    //            TwoCharWrapPos = new Point(480, 981)
    //        };
    //        WrapTest(Testcase);
    //    }
    //
    //    [TestMethod]
    //    public void HLTRWrapTest()
    //    {
    //        var Testcase = new WrapTestcase()
    //        {
    //            NoWrapPos = new Point(480, 0),
    //            Direction = PageRenderer.WritingFlow.VRTL,
    //            SingleCharWrapPos = new Point(480, 991),
    //            SingleCharNearlyWrapPos = new Point(480, 990),
    //            TwoCharWrapPos = new Point(480, 981)
    //        };
    //        WrapTest(Testcase);
    //    }

        private void WrapTest(WrapTestcase testcase)
        {
            renderer.Direction =testcase.Direction;
            renderer.CurrentWritePosition = testcase.NoWrapPos;
            Assert.IsTrue(!renderer.NeedsToWrap(1));
            Assert.IsTrue(!renderer.NeedsToWrap(2));
            renderer.CurrentWritePosition = testcase.SingleCharWrapPos;
            Assert.IsTrue(renderer.NeedsToWrap(1));
            Assert.IsTrue(renderer.NeedsToWrap(2));
            renderer.CurrentWritePosition = testcase.SingleCharNearlyWrapPos;
            Assert.IsTrue(!renderer.NeedsToWrap(1));
            Assert.IsTrue(renderer.NeedsToWrap(2));
            renderer.CurrentWritePosition = testcase.TwoCharWrapPos;
            Assert.IsTrue(!renderer.NeedsToWrap(1));
            Assert.IsTrue(renderer.NeedsToWrap(2));
        }

        private class WrapTestcase
        {
            public Point NoWrapPos;
            public Point SingleCharNearlyWrapPos;
            public Point SingleCharWrapPos;
            public Point TwoCharWrapPos;
            public PageRenderer.WritingFlow Direction;
        }
    }
}
