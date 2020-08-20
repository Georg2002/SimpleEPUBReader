using EPUBParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public class PageRenderer : FrameworkElement
    {
        public static readonly Typeface Typeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));
        private static readonly Rect ErrorRect = new Rect(new Point(), new Point(200, 200));

        //CurrentWritePosition must always be at the point on the corner of the last added part that is closest to the limitations
        //VRTL: bottom left, VLTR: bottom right, HRTL: bottom left, HLTR: bottom right 

        public Vector CurrentWritePos;

        public Vector PageSize;

        private List<Writing> TextParts;
        private List<ImageInText> Images;
        private EpubSettings Settings;

        public PageRenderer(EpubSettings pageSettings, double Width, double Height)
        {
            this.PageSize = new Vector(Width, Height);
            this.Settings = pageSettings;
        }

        protected override void OnRender(DrawingContext Context)
        {
            Width = PageSize.X;
            Height = PageSize.Y;

            foreach (var TextPart in TextParts)
            {
                FormattedText Text = GetText(TextPart);
                Context.DrawText(Text, new Point(TextPart.RenderPosition.X, TextPart.RenderPosition.Y));
            }
        }

        private FormattedText GetText(Writing textPart)
        {
            return new FormattedText(textPart.ToString(),
                 CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                 Typeface, textPart.FontSize, Brushes.Black, 1);
        }

        public bool Fits(ImageLinePart part)
        {
            ImageSource img = part.GetImage();
            Vector Dimensions;
            if (img == null)
            {
                Dimensions = new Vector(ErrorRect.Width, ErrorRect.Height);
            }
            else
            {
                Dimensions = new Vector(img.Width, img.Height);
            }
            Vector AfterImagePos = FlowDirectionModifiers.GetAfterImagePosition(CurrentWritePos, PageSize, Dimensions);
            return InPage(AfterImagePos);
        }

        public bool InPage(Vector WritePos)
        {
            throw new NotImplementedException();
        }

        public List<Writing> GetMainTextWritings(string word)
        {
            Vector WritingPosCopy = CurrentWritePos;
            List<Writing> Result = new List<Writing>();
            bool HadFix = false;
            Vector StartOffset = new Vector();
            Vector EndOffset = new Vector();
            foreach (char c in word)
            {
                var NewItem = new Writing();
                if (Settings.Vertical)
                {
                    if (GlobalSettings.VerticalVisualFixes.ContainsKey(c))
                    {
                        var Info = GlobalSettings.VerticalVisualFixes[c];
                        NewItem.Text = Info.Replacement;
                        StartOffset = Info.StartOffset;
                        EndOffset = Info.EndOffset;
                        HadFix = true;
                    }
                    else
                    {
                        NewItem.Text = c;
                    }
                }
                else
                {
                    NewItem.Text = c;
                }

                NewItem.WritingPosition = WritingPosCopy;

                if (HadFix)
                {
                    NewItem.WritingPosition += StartOffset * ChapterPagesCreator.FontSize;
                }
                NewItem.RenderPosition = FlowDirectionModifiers.GetRenderPos(NewItem.WritingPosition);

                WritingPosCopy = FlowDirectionModifiers.GetAfterWritingPosition(WritingPosCopy, NewItem);
                if (HadFix)
                {
                    WritingPosCopy += EndOffset * ChapterPagesCreator.FontSize;
                }
            }

            foreach (var Writing in Result)
            {
                FlowDirectionModifiers.WrapIntoPage(Writing);
            }
            return Result;
        }

        internal void Write(TextLinePart textPart)
        {
            throw new NotImplementedException();
        }

        public void AddImage(LinePart part)
        {
            throw new NotImplementedException();
        }

        public void Write(string word)
        {
            throw new NotImplementedException();
        }
    }


    public class ImageInText
    {
        public ImageSource Image;
        public Point RenderPosition;
    }

    public class Writing
    {
        public char Text;
        public double FontSize;
        public Vector RenderPosition;
        public Vector WritingPosition;
    }
}
