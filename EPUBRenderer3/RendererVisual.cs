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
            if (ShownPage == null) return;

            foreach (var Line in ShownPage.Lines)
            {
                foreach (var Word in Line.Words)
                {
                    foreach (var Letter in Word.Letters)
                    {
                        if (Letter.MarkingColorIndex != 0)
                        {
                            double Width = Letter.EndPosition.X - Letter.StartPosition.Y;
                            double Height = Letter.EndPosition.Y - Letter.StartPosition.Y;
                            var R = new Rect(Letter.StartPosition.X,Letter.StartPosition.Y, Width, Height);
                            drawingContext.DrawRectangle(MarkingColors[Letter.MarkingColorIndex], null, R);
                        }


                        switch (Letter.Type)
                        {
                            case LetterTypes.Letter:
                                var Text = (FormattedText)Letter.GetRenderElement();
                                var TxtLetter = (TextLetter)Letter;
                                var DrawPos = Letter.StartPosition + TxtLetter.Offset * TxtLetter.FontSize; ;
                                drawingContext.DrawText(Text, new Point(DrawPos.X, DrawPos.Y));
                                break;
                            case LetterTypes.Image:
                                var ImgLetter = (ImageLetter)Letter;
                                var Img = (ImageSource)Letter.GetRenderElement();                               
                                if (ShownPage.Lines.Count == 1 && Line.Words.Count == 1 && Word.Letters.Count == 1)
                                {
                                    Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                    ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                    ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;                                    
                                }
                                var StartPoint = new Point(Letter.StartPosition.X, Letter.StartPosition.Y);
                                var EndPoint = new Point(Letter.EndPosition.X, Letter.EndPosition.Y);
                                drawingContext.DrawImage(Img, new Rect(StartPoint, EndPoint));
                                break;
                            case LetterTypes.Break:
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }
        }
    }
}
