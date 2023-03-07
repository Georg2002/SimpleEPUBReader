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

namespace EPUBRenderer
{

    public partial class Renderer : FrameworkElement
    {
        public static extern void 
        private void drawCharAt(GlyphRun run, Point topCenter)
        {
            //    dc.PushTransform(new TranslateTransform(topCenter.X, topCenter.Y));
            run.GlyphOffsets.Add(new Point(-topCenter.X, -topCenter.Y));
            //draw called at the end of the loop
            //    dc.Pop();

        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;

            if (ShownPage == null || !Rerender) return;

            TextLetter.ClearRuns();

            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));//force update maybe

            bool SingleImage = ShownPage.IsSingleImage();
            foreach (var Let in ShownPage.Content)
            {
                switch (Let.Type)
                {
                    case LetterTypes.Letter:
                        var run = (GlyphRun)Let.GetRenderElement();
                        var txtLetter = (TextLetter)Let;
                        var DrawPos = Let.StartPosition + txtLetter.Offset * txtLetter.FontSize;
                        DrawPos.Y -= txtLetter.FontSize * CharInfo.FontOffset;

                        Point offset = new(DrawPos.X - txtLetter.FontSize / 2, DrawPos.Y);

                        if (txtLetter.Rotated) drawingContext.PushTransform(new RotateTransform(txtLetter.Rotation, txtLetter.Middle.X, txtLetter.Middle.Y));
                        this.drawCharAt(run, offset);
                        if (txtLetter.Rotated) drawingContext.Pop();
                        break;
                    case LetterTypes.Image:
                        var ImgLetter = (ImageLetter)Let;
                        var Img = (ImageSource)ImgLetter.GetRenderElement();
                        var StartPoint = ImgLetter.GetStartPoint();
                        var EndPoint = ImgLetter.GetEndPoint();
                        if (Img == null)
                        {
                            var RedPen = new Pen(Brushes.Red, 1);
                            drawingContext.DrawRectangle(null, RedPen, ImgLetter.GetImageRect());
                            drawingContext.DrawLine(RedPen, StartPoint, EndPoint);
                            drawingContext.DrawLine(RedPen, new Point(StartPoint.X, EndPoint.Y), new Point(EndPoint.X, StartPoint.Y));
                        }
                        else
                        {
                            if (SingleImage)
                            {
                                Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;
                            }
                            drawingContext.DrawImage(Img, ImgLetter.GetImageRect());
                        }
                        break;
                    case LetterTypes.Break:
                    case LetterTypes.Marker:
                        break;
                    default:
                        throw new NotImplementedException();
                }

            }

            TextLetter.DrawRuns(drawingContext);

            foreach (var letter in ShownPage.Content.Where(Let => Let.MarkingColorIndex != 0 || (Let.DictSelected && !Let.IsRuby)))
            {
                var Rect = letter.GetMarkingRect();
                if (letter.MarkingColorIndex != 0) drawingContext.DrawRectangle(MarkingColors[letter.MarkingColorIndex], null, Rect);
                if (letter.DictSelected && !letter.IsRuby) drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
            }

            int Total = GetPageCount();
            int Current = GetCurrentPage();
            FormattedText PageText = new FormattedText($"{Current}/{Total}", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, CharInfo.StandardTypeface, 15, Brushes.Black, 1);
            double Width = PageText.Width;
            drawingContext.DrawText(PageText, new Point((PageSize.X - Width) / 2, PageSize.Y + 10));

            Rerender = false;
        }

        public void ResetSelection()
        {
            SelectionStart = PosDef.InvalidPosition;
            SelectionEnd = PosDef.InvalidPosition;
        }
    }
}
