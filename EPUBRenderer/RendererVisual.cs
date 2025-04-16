using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Windows.Media.TextFormatting;
using EPUBRenderer;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Security.Policy;
using System.Windows.Input;
using EPUBParser;

namespace EPUBRenderer
{

    public partial class Renderer : HwndHost
    {
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SetWindow(IntPtr windowHandle);
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr DestroyW();
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void HandleMessages(uint msg, IntPtr wparam, IntPtr lparam);
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void BeginDraw();
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void EndDraw();
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void DrawCharacter(float x, float y, uint character, float size, bool bold, float rotation);

        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void drawImage(byte[] data, int imageSize, float x0, float y0, float x1, float y1);
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void drawMissingImage(float x0, float y0, float x1, float y1);
        [DllImport("RenderSupport.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void DrawMarkingRect(bool isMarked, int colorIndex, bool isSelected, float x0, float y0, float x1, float y1);
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var ptr = SetWindow(hwndParent.Handle);
            return new HandleRef(this, ptr);
        }
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyW();
        }
        IntPtr childWindow;
        private void drawCharAt(char character, float size, bool bold, float rotation, Point topCenter)
        {
            DrawCharacter((float)topCenter.X, (float)topCenter.Y, character, size, bold, rotation);
        }
        private void DrawImage(byte[] data, Rect rect)
        {
            drawImage(data, data.Length, (float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage is null || !Rerender) return;
            BeginDraw();
            bool SingleImage = ShownPage.IsSingleImage();
            foreach (var Let in ShownPage.Content)
            {
                switch (Let.Type)
                {
                    case LetterTypes.Letter:
                        var run = (GlyphRun)Let.GetRenderElement();
                        var txtLetter = (TextLetter)Let;
                        var DrawPos = Let.StartPosition + txtLetter.Offset * txtLetter.FontSize;
                        //DrawPos.Y -= txtLetter.FontSize * CharInfo.FontOffset;

                        Point offset = new(DrawPos.X, DrawPos.Y);

                        this.drawCharAt(txtLetter.Character, txtLetter.FontSize, txtLetter.Style.Weight != System.Windows.FontWeights.Normal, (float)txtLetter.Rotation, offset);
                        break;
                    case LetterTypes.Image:
                        var ImgLetter = (ImageLetter)Let;
                        var Img = (ImageObject)ImgLetter.GetRenderElement();
                        var StartPoint = ImgLetter.GetStartPoint();
                        var EndPoint = ImgLetter.GetEndPoint();
                        if (Img == null)
                        {
                            drawMissingImage((float)StartPoint.X, (float)EndPoint.Y, (float)EndPoint.X, (float)StartPoint.Y);
                            /*
                            var RedPen = new Pen(Brushes.Red, 1);
                            drawingContext.DrawRectangle(null, RedPen, ImgLetter.GetImageRect());
                            drawingContext.DrawLine(RedPen, StartPoint, EndPoint);
                            drawingContext.DrawLine(RedPen, new Point(StartPoint.X, EndPoint.Y), new Point(EndPoint.X, StartPoint.Y));
                            */
                        }
                        else
                        {
                            if (SingleImage)
                            {
                                Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;
                            }
                            this.DrawImage(Img.Data, ImgLetter.GetImageRect());
                            //drawingContext.DrawImage(Img, ImgLetter.GetImageRect());
                        }
                        break;
                    case LetterTypes.Break:
                    case LetterTypes.Marker:
                        break;
                    default:
                        throw new NotImplementedException();
                }

            }
            

            foreach (var letter in ShownPage.Content.Where(Let => Let.MarkingColorIndex != 0 || (Let.DictSelected && !Let.IsRuby)))
            {
                var Rect = letter.GetMarkingRect();
                var isMarked = letter.MarkingColorIndex != 0;
                var isDictSelected = letter.DictSelected && !letter.IsRuby;
                DrawMarkingRect(isMarked, letter.MarkingColorIndex, isDictSelected, (float)Rect.Left, (float)Rect.Top, (float)Rect.Right, (float)Rect.Bottom);

                //      if (letter.MarkingColorIndex != 0) drawingContext.DrawRectangle(MarkingColors[letter.MarkingColorIndex], null, Rect);
                 //      if (letter.DictSelected && !letter.IsRuby) drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
            }

            int Total = GetPageCount();
            int Current = GetCurrentPage();
            FormattedText PageText = new FormattedText($"{Current}/{Total}", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, CharInfo.StandardTypeface, 15, Brushes.Black, 1);
            double Width = PageText.Width;
            //  drawingContext.DrawText(PageText, new Point((PageSize.X - Width) / 2, PageSize.Y + 10));
            Rerender = false;
            EndDraw();
        }

        public void ResetSelection()
        {
            SelectionStart = PosDef.InvalidPosition;
            SelectionEnd = PosDef.InvalidPosition;
        }
    }
}
