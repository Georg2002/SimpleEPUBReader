using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public class PageRenderer : FrameworkElement
    {
        public RenderPage Page;
        public Vector PageSize;

        protected override void OnRender(DrawingContext Context)
        {
            if (Page == null) return;
            var Text = WritingDirectionModifiers.GetTextInPage(Page, PageSize);
            Vector Offset;
            var First = Text.First();
            if (Page.CurrentPage == 1)
            {
                Offset = new Vector();
            }
            else
            {
                if (First.ElementType == TextElementType.Image)
                {
                    var StartPos = WritingDirectionModifiers.GetImageStartPos((ImageInText)First, WritingDirectionModifiers.GetStartPosition(PageSize), PageSize);
                    Offset = StartPos - First.StartPos;
                }
                else
                {
                    Offset = WritingDirectionModifiers.GetStartPosition(PageSize) - First.StartPos;
                }
            }
            Page.CurrentOffset = Offset;
            foreach (var Element in Text)
            {
                switch (Element.ElementType)
                {
                    case TextElementType.Letter:
                    case TextElementType.RubyLetter:
                        var Letter = (Letter)Element;
                        Point WritingPosition = WritingDirectionModifiers.GetWritingPosition(Letter);
                        WritingPosition.Offset(Offset.X, Offset.Y);
                        Context.DrawText(Letter.FormattedText, WritingPosition);
                        break;
                    case TextElementType.Image:
                        var Image = (ImageInText)Element;
                        Point StartPoint = new Point(Element.StartPos.X, Element.StartPos.Y);
                        Point EndPoint = new Point(Element.EndPos.X, Element.EndPos.Y);
                        if (Text.Count == 1)
                        {
                            Offset.X = (PageSize.X - Image.Size.X) / 2 - StartPoint.X;
                            Offset.Y = (PageSize.Y - Image.Size.Y) / 2 - StartPoint.Y;
                        }
                        StartPoint.Offset(Offset.X, Offset.Y);
                        EndPoint.Offset(Offset.X, Offset.Y);
                        Rect Rect = new Rect(StartPoint, EndPoint);

                        if (Image.Image == null)
                        {
                            Rect.Width = GlobalSettings.ErrorRect.Width;
                            Rect.Height = GlobalSettings.ErrorRect.Height;
                            Context.DrawRectangle(Brushes.Red, null, Rect);
                        }
                        else
                        {
                            Context.DrawImage(Image.Image, Rect);
                        }
                        break;
                }
                if (Element.MarkingColor != null)
                {
                    Point StartPoint = new Point(Element.StartPos.X, Element.StartPos.Y);
                    StartPoint.Offset(Offset.X, Offset.Y);
                    Point EndPoint = new Point(Element.EndPos.X, Element.EndPos.Y);
                    EndPoint.Offset(Offset.X, Offset.Y);
                    if (Element.ElementType == TextElementType.Letter ||
                        Element.ElementType == TextElementType.RubyLetter)
                    {
                        double FontOffset = ((Letter)Element).FontSize * -0.1;
                        StartPoint.Offset(0, FontOffset);
                        EndPoint.Offset(0, FontOffset);
                    }
                    Context.DrawRectangle(Element.MarkingColor, null, new Rect(StartPoint, EndPoint));
                }
            }
            Width = PageSize.X;
            Height = PageSize.Y;
        }
    }
}
