using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public class PageRenderer2 : FrameworkElement
    {
        private WritingDirection WritingDirection;
        public WritingFlow Direction;

        public static readonly Typeface Typeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));

        //CurrentWritePosition must always be at the point on the corner of the last added part that is closest to the limitations
        //VRTL: bottom left, VLTR: bottom right, HRTL: bottom left, HLTR: bottom right
        public ChapterPosition CurrentTextPos;

        public Point CurrentWritePosition;

        public double PageWidth;
        public double PageHeight;

        private List<Writing> TextParts;
        private List<ImageInText> Images;
        private EpubSettings pageSettings;

        public PageRenderer2(EpubSettings pageSettings)
        {
            this.pageSettings = pageSettings;
        }

        protected override void OnRender(DrawingContext Context)
        {
            foreach (var TextPart in TextParts)
            {
                Context.DrawText(TextPart.Text, TextPart.RenderPosition);
            }
        }

        public bool Fits(LinePart part)
        {
            throw new NotImplementedException();
        }

        internal void Write(TextLinePart textPart)
        {
            throw new NotImplementedException();
        }

        public void AddImage(LinePart part)
        {
            throw new NotImplementedException();
        }

        internal bool Fits(string word)
        {
            throw new NotImplementedException();
        }

        internal void Write(string word)
        {
            throw new NotImplementedException();
        }
    }


    public class ImageInText
    {
        public ImageSource Image;
        public Point RenderPosition;
        public Point WritingPosition;
    }

    public class Writing
    {
        public FormattedText Text;
        public Point RenderPosition;
        public Point WritingPosition;
    }
}
