using System;
using System.Windows;
using EPUBRenderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.RendererTests
{
    [TestClass]
    public class WrapTests
    {
        PageRenderer renderer;
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
            renderer.Direction = WritingFlow.VRTL;
            renderer.CurrentWritePosition = new Point(480,1000);
            WritingDirection.Wrap();
            Assert.IsTrue(renderer.CurrentWritePosition.X == 460);
            Assert.IsTrue(renderer.CurrentWritePosition.Y == 0);
        }

        [TestMethod]
        public void VLTRWrapTest()
        {
            renderer.Direction = WritingFlow.VLTR;
            renderer.CurrentWritePosition = new Point(20, 1000);
            WritingDirection.Wrap();
            Assert.IsTrue(renderer.CurrentWritePosition.X == 40);
            Assert.IsTrue(renderer.CurrentWritePosition.Y == 0);
        }
    }
}
