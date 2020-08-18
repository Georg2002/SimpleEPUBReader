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
            renderer.Direction = PageRenderer.WritingFlow.VRTL;
            renderer.CurrentWritePosition = new Point(480,1000);
            renderer.Wrap();
            Assert.IsTrue(renderer.CurrentWritePosition.X == 460  );
            Assert.IsTrue(renderer.CurrentWritePosition.Y == 0);
        }

        [TestMethod]
        public void VLTRWrapTest()
        {
            renderer.Direction = PageRenderer.WritingFlow.VLTR;
            renderer.CurrentWritePosition = new Point(20, 1000);
            renderer.Wrap();
            Assert.IsTrue(renderer.CurrentWritePosition.X == 40);
            Assert.IsTrue(renderer.CurrentWritePosition.Y == 0);
        }
    }
}
