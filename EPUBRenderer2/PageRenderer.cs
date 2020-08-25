using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer2
{
    public class PageRenderer : FrameworkElement
    {
        public RenderPage Page;
        public Vector PageSize;     

        protected override void OnRender(DrawingContext Context)
        {
            if (Page == null) return;

            var Words = Page.TextElements;
           
            var ShownWords = WritingDirectionModifiers.GetWordsInPage(Words, Page.Offset, PageSize);

            foreach (var Word in ShownWords)
            {
                foreach (var Element in Word)
                {
                    switch (Element.ElementType)
                    {
                        case TextElementType.Letter:
                        case TextElementType.RubyLetter:
                            var Letter = (Letter)Element;
                            Point WritingPosition = WritingDirectionModifiers.GetWritingPosition(Letter);
                            Context.DrawText(Letter.FormattedText, WritingPosition);
                            break;
                        case TextElementType.Image:
                            var Image = (ImageInText)Element;
                            var Rect = new Rect(new Point(Image.StartPos.X, Image.StartPos.Y), new Point(Image.EndPos.X, Image.EndPos.Y));
                            Context.DrawImage(Image.Image, Rect);
                            break;
                    }
                }
            }
            Width = PageSize.X;
            Height = PageSize.Y;
            Context.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Red, 2), new Rect(0, 0, Width, Height));
        }
    }
}
