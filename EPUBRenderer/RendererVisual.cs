using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Point = System.Windows.Point;
using Brushes = System.Drawing.Brushes;
using Pen = System.Drawing.Pen;
using System.Windows.Forms;
using Microsoft.Win32;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;

namespace EPUBRenderer
{
    public partial class Renderer : FrameworkElement
    {
        private StringFormat format;
        WriteableBitmap Bitmap = new WriteableBitmap(4000, 4000, 96, 96, pixelFormat: PixelFormats.Bgr32, null);
        Bitmap bm;
        Graphics graphics;
        IntPtr hdc;


        private IntPtr lastFont = IntPtr.Zero;
        private void drawTextAt(string text, IntPtr font, System.Drawing.Point topCenter)
        {
            if (font != this.lastFont)
            {
                this.lastFont = font;
                Win32Func.SelectObject(hdc, font);
            }
            Win32Func.TextOut(hdc, topCenter.X, topCenter.Y, text, text.Length);
            //TextRenderer.DrawText(this.graphics, text, font,
            //   new System.Drawing.Rectangle(topCenter.X - 200, topCenter.Y, 400, 200), CharInfo.Black, CharInfo.White, TextFormatFlags.HorizontalCenter);

        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage == null || !Rerender) return;
            Bitmap.Lock();
            bm ??= new(Bitmap.PixelWidth, Bitmap.PixelHeight, Bitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, Bitmap.BackBuffer);
            format ??= new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
            if (graphics is null)
            {
                graphics = Graphics.FromImage(bm);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            }
            graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, (int)this.PageSize.X + 300, (int)PageSize.Y + 300));
            this.hdc = graphics.GetHdc();

            bool SingleImage = ShownPage.IsSingleImage();
            foreach (var Let in ShownPage.Content)
            {
                switch (Let.Type)
                {
                    case LetterTypes.Letter:
                        var fontPtr = (IntPtr)Let.GetRenderElement(graphics);
                        var txtLetter = (TextLetter)Let;
                        var DrawPos = Let.StartPosition + txtLetter.Offset * txtLetter.FontSize;
                        DrawPos.Y += txtLetter.FontSize * CharInfo.FontOffset;

                        System.Drawing.Point offset = new((int)(DrawPos.X - txtLetter.FontSize / 2), (int)(DrawPos.Y + txtLetter.FontSize / 2));

                        if (txtLetter.Rotated && false)
                        {
                            graphics.TranslateTransform((float)txtLetter.Middle.X, (float)txtLetter.Middle.Y);
                            graphics.RotateTransform((float)txtLetter.Rotation);
                            graphics.TranslateTransform(-(float)txtLetter.Middle.X, -(float)txtLetter.Middle.Y);
                        }
                        this.drawTextAt(txtLetter.Character.ToString(), fontPtr, offset);
                        //graphics.DrawString(txtLetter.Character.ToString(), Font, Brushes.Black, offset, format);
                        //graphics.DrawEllipse(Pens.Red, (int)DrawPos.X - 2, (int)DrawPos.Y - 2, 4, 4);
                        if (txtLetter.Rotated && false) graphics.ResetTransform();
                        break;
                    case LetterTypes.Image:
                        graphics.ReleaseHdc();
                        var ImgLetter = (ImageLetter)Let;
                        var Img = (Image)ImgLetter.GetRenderElement(graphics);
                        var StartPoint = ImgLetter.GetStartPoint();
                        var EndPoint = ImgLetter.GetEndPoint();
                        if (Img == null)
                        {
                            var RedPen = new Pen(Brushes.Red, 1);
                            graphics.DrawRectangle(RedPen, ImgLetter.GetImageRect());
                            graphics.DrawLine(RedPen, StartPoint, EndPoint);
                            graphics.DrawLine(RedPen, new PointF(StartPoint.X, EndPoint.Y), new PointF(EndPoint.X, StartPoint.Y));
                        }
                        else
                        {
                            if (SingleImage)
                            {
                                Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;
                            }
                            graphics.DrawImage(Img, ImgLetter.GetImageRect());
                        }
                        this.hdc = graphics.GetHdc();
                        break;
                    case LetterTypes.Break:
                    case LetterTypes.Marker:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            int Total = GetPageCount();
            int Current = GetCurrentPage();
            this.drawTextAt($"{Current}/{Total}", CharInfo.StandardFont, new System.Drawing.Point((int)(PageSize.X / 2), (int)(PageSize.Y + 10)));
            //graphics.DrawString($"{Current}/{Total}", CharInfo.StandardFont, Brushes.Black, , format);
            this.graphics.ReleaseHdc();

            //gdi+ has opacity
            foreach (var letter in ShownPage.Content.Where(a => a.MarkingColorIndex != 0 || a.DictSelected))
            {
                var Rect = letter.GetMarkingRect();
                if (letter.MarkingColorIndex != 0) graphics.FillRectangle(MarkingColors[letter.MarkingColorIndex], Rect);
                if (letter.DictSelected && !letter.IsRuby) graphics.FillRectangle(Letter.DictSelectionColor, Rect);
            }

            Bitmap.AddDirtyRect(new Int32Rect(0, 0, (int)this.PageSize.X + 300, (int)PageSize.Y + 300));
            Bitmap.Unlock();
            drawingContext.DrawImage(Bitmap, new Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));

            Rerender = false;
        }

        public void ResetSelection()
        {
            SelectionStart = PosDef.InvalidPosition;
            SelectionEnd = PosDef.InvalidPosition;
        }
    }
}
