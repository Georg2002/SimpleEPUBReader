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
        private WritingDirection WritingDirection;

        [TestInitialize]
        public void Init()
        {
              renderer = TestResources.CommonInit();
            WritingDirection = renderer.WritingDirection;
        }

        [TestMethod]
        public void VRTLWrapTest()
        {
            var Testcase = new WrapTestcase()
            {
                NoWrapPos = new Point(480, 0),
                Direction = WritingFlow.VRTL,
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
                Direction = WritingFlow.VRTL,
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
            Assert.IsTrue(!WritingDirection.NeedsToWrap(1));
            Assert.IsTrue(!WritingDirection.NeedsToWrap(2));
            renderer.CurrentWritePosition = testcase.SingleCharWrapPos;
            Assert.IsTrue(WritingDirection.NeedsToWrap(1));
            Assert.IsTrue(WritingDirection.NeedsToWrap(2));
            renderer.CurrentWritePosition = testcase.SingleCharNearlyWrapPos;
            Assert.IsTrue(!WritingDirection.NeedsToWrap(1));
            Assert.IsTrue(WritingDirection.NeedsToWrap(2));
            renderer.CurrentWritePosition = testcase.TwoCharWrapPos;
            Assert.IsTrue(!WritingDirection.NeedsToWrap(1));
            Assert.IsTrue(WritingDirection.NeedsToWrap(2));
        }

        private class WrapTestcase
        {
            public Point NoWrapPos;
            public Point SingleCharNearlyWrapPos;
            public Point SingleCharWrapPos;
            public Point TwoCharWrapPos;
            public WritingFlow Direction;
        }
    }
}
