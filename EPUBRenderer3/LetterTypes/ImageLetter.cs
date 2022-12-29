﻿using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
    internal class ImageLetter : Letter
    {
        public ImageSource Image;
        public bool Inline;
        public override float FontSize => (float)this.Width;
        public ImageLetter(ImageSource Image, bool Inline)
        {
            Type = LetterTypes.Image;
            this.Inline = Inline;
            this.Image = Image;
        }

        private double Width;
        private double Height;

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;
            var PrevWord = Info.PrevWord;
            if (Image == null) Width = Height = 300;
            else
            {
                var style = Info.OwnWord.Style;
                this.Width = style.Width.HasValue ? style.Width.Value: Image.Width;
                this.Height = style.Height.HasValue ? style.Height.Value : Image.Height;
                double ratio = Image.Height / Image.Width;
                if (style.Width.HasValue) this.Height = ratio * this.Width;
                else if (style.Height.HasValue) this.Width = this.Height / ratio;
            } 

            bool MustScale = PageSize.X < Width || PageSize.Y < Height;
            StartPosition = PrevLetter == null ? new Vector(PageSize.X, 0) : new Vector(PrevLetter.EndPosition.X, 0);
            Vector RenderSize = new Vector(-Width, Height);
            if (Inline)
            {
                double Scale = LineDist <= Width ? LineDist / Width : 1;
                RenderSize *= Scale;
                StartPosition = PrevLetter == null ? StartPosition : PrevLetter.NextWritePos;
                if (Info.NewLine)
                {
                    StartPosition.X -= LineDist;
                    StartPosition.Y = 0;
                    PrevLetter = null;
                }
                float FS = StandardFontSize;
                if (PrevLetter != null && PrevLetter.Type == LetterTypes.Letter) FS = ((TextLetter)PrevLetter).FontSize;
                StartPosition += new Vector(-(FS + RenderSize.X) / 2, 0);
                EndPosition = StartPosition + RenderSize;
                NextWritePos = PrevLetter == null ? new Vector(StartPosition.X + (FS + RenderSize.X) / 2, EndPosition.Y) : PrevLetter.NextWritePos + new Vector(0, RenderSize.Y);
            }
            else
            {
                if (MustScale)
                {
                    if (PrevWord != null) return false;
                    RenderSize = GetMaxRenderSize(PageSize);
                    StartPosition = (PageSize - RenderSize) / 2;
                }
                else StartPosition.Y = (PageSize.Y - Height) / 2;
             
                EndPosition = StartPosition + RenderSize;
                NextWritePos = MustScale ? new Vector(-1, PageSize.Y + 1) : new Vector(EndPosition.X - LineDist, 0);
            }

            return InsidePage(PageSize);
        }

        public Point GetStartPoint()
        {
            return new Point(StartPosition.X, StartPosition.Y);
        }

        public Point GetEndPoint()
        {
            return new Point(EndPosition.X, EndPosition.Y);
        }

        public Rect GetImageRect()
        {
            return new Rect(GetStartPoint(), GetEndPoint());
        }

        public Vector GetMaxRenderSize(Vector PageSize)
        {
            double PRatio = PageSize.X / PageSize.Y;
            double IRatio = Width / Height;
            return PRatio < IRatio ? new Vector(-PageSize.X, PageSize.X / IRatio) : new Vector(-PageSize.Y * IRatio, PageSize.Y);
        }

        public override object GetRenderElement(bool KatakanaLearningMode)
        {
            return Image;
        }
    }
}
