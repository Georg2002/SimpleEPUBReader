using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EPUBRenderer
{

    public partial class Renderer : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage == null || !Rerender) return;
            bool SingleImage = ShownPage.IsSingleImage();
            foreach (var Let in ShownPage.Content)
            {
                switch (Let.Type)
                {
                    case LetterTypes.Letter:
                        var Text = (FormattedText)Let.GetRenderElement();
                        var txtLetter = (TextLetter)Let;
                        var DrawPos = Let.StartPosition + txtLetter.Offset * txtLetter.FontSize;
                        DrawPos.Y -= txtLetter.FontSize * CharInfo.FontOffset;
                        if (txtLetter.Rotated) drawingContext.PushTransform(new RotateTransform(txtLetter.Rotation, txtLetter.Middle.X, txtLetter.Middle.Y));
                        Point offset = new Point(DrawPos.X - txtLetter.FontSize / 2, DrawPos.Y);
                        drawingContext.DrawText(Text, new Point(DrawPos.X - txtLetter.FontSize / 2, DrawPos.Y));
                        if (txtLetter.Rotated) drawingContext.Pop();
                        if (txtLetter.DictSelected && !txtLetter.IsRuby)
                        {
                            var Rect = Let.GetMarkingRect();
                            drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
                        }
                        break;
                    case LetterTypes.Image:
                        var ImgLetter = (ImageLetter)Let;
                        var Img = (ImageSource)ImgLetter.GetRenderElement();
                        var StartPoint = ImgLetter.GetStartPoint();
                        var EndPoint = ImgLetter.GetEndPoint();
                        if (Img == null)
                        {
                            var RedPen = new Pen(Brushes.Red, 1);
                            drawingContext.DrawRectangle(Brushes.Transparent, RedPen, ImgLetter.GetImageRect());
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
                if (Let.MarkingColorIndex != 0)
                {
                    var Rect = Let.GetMarkingRect();
                    drawingContext.DrawRectangle(MarkingColors[Let.MarkingColorIndex], null, Rect);
                }
            }
            int Total = GetPageCount();
            int Current = GetCurrentPage();
            var PageText = new FormattedText($"{Current}/{Total}", CultureInfo.InvariantCulture,
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
