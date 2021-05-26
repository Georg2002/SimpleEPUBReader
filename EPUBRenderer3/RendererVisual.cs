using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{

    public partial class Renderer : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage == null ||!Rerender) return;

            bool SingleImage = ShownPage.IsSingleImage();

            foreach (var Line in ShownPage.Lines)
            {
                foreach (var Word in Line.Words)
                {
                    foreach (var Let in Word.Letters)
                    {           
                        switch (Let.Type)
                        {
                            case LetterTypes.Letter:
                                var Text = (FormattedText)Let.GetRenderElement();
                                var TxtLetter = (TextLetter)Let;
                                var DrawPos = Let.StartPosition + TxtLetter.Offset * TxtLetter.FontSize;
                                DrawPos.Y -= TxtLetter.FontSize * CharInfo.FontOffset;
                                drawingContext.DrawText(Text, new Point(DrawPos.X - TxtLetter.FontSize / 2, DrawPos.Y));
                                if (TxtLetter.DictSelected && TxtLetter.FontSize == TextLetter.StandardFontSize)
                                {
                                    var Rect = Let.GetMarkingRect();
                                    drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
                                }
                                break;
                            case LetterTypes.Image:
                                var ImgLetter = (ImageLetter)Let;
                                var Img = (ImageSource)Let.GetRenderElement();                               
                                if (SingleImage)
                                {
                                    Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                    ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                    ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;                                    
                                }
                                var StartPoint = new Point(Let.StartPosition.X, Let.StartPosition.Y);
                                var EndPoint = new Point(Let.EndPosition.X, Let.EndPosition.Y);
                                drawingContext.DrawImage(Img, new Rect(StartPoint, EndPoint));
                               
                                break;
                            case LetterTypes.Break:
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        if (Let.MarkingColorIndex != 0)
                        {
                            var Rect = Let.GetMarkingRect();
                            drawingContext.DrawRectangle(MarkingColors[Let.MarkingColorIndex], null,Rect );
                        }                       
                    }
                }
            }
            int Total = GetPageCount();
            int Current = GetCurrentPage();
            FormattedText PageText = new FormattedText($"{Current}/{Total}", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, CharInfo.StandardTypeface, 15, Brushes.Black,1);
            double Width = PageText.Width;
            drawingContext.DrawText(PageText, new Point((PageSize.X - Width) / 2, PageSize.Y + 10));
            Rerender = false;
        }
    }
}
