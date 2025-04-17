using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace EPUBRenderer
{
    internal class ImageLetter : Letter
    {
        public ImageSource Image;
        public bool Inline;
        public ImageLetter(ImageSource Image, bool Inline, WordInfo wordInfo) : base(wordInfo)
        {
            Type = LetterTypes.Image;
            this.Inline = Inline;
            this.IsWordEnd = true;
            this.Image = Image;
        }

        private double Width;
        private double Height;

        public override bool Position(LetterPlacementInfo Info)
        {
            var PageSize = Info.PageSize;
            if (Image == null) Width = Height = 300;
            else
            {
                this.Width = this.Style.Width ?? this.Image.Width;
                this.Height = this.Style.Height ?? this.Image.Height;
                double ratio = this.Image.Height / this.Image.Width;
                if (this.Style.Width.HasValue) this.Height = ratio * this.Width;
                else if (this.Style.Height.HasValue) this.Width = this.Height / ratio;
            }

            this.FontSize = (float)this.Width;

            bool MustScale = PageSize.X < Width || PageSize.Y < Height;
            StartPosition = IsPageStart ? new Vector(PageSize.X, 0) : new Vector(this.PrevLetter.EndPosition.X, 0);
            Vector RenderSize = new Vector(-Width, Height);
            if (Inline)
            {
                double Scale = LineDist <= Width ? LineDist / Width : 1;
                RenderSize *= Scale;
                StartPosition = IsPageStart ? StartPosition : this.PrevLetter.NextWritePos;
                if (Info.State == PositionState.Newline)
                {
                    this.StartPosition.X -= LineDist;
                    this.StartPosition.Y = 0;
                    IsPageStart = true;
                }
                float FS = StandardFontSize;
                if (!IsPageStart && this.PrevLetter.Type == LetterTypes.Letter) FS = ((TextLetter)PrevLetter).FontSize;
                StartPosition += new Vector(-(FS + RenderSize.X) / 2, 0);
                EndPosition = StartPosition + RenderSize;
                NextWritePos = IsPageStart ? new Vector(this.StartPosition.X + (FS + RenderSize.X) / 2, this.EndPosition.Y) : this.PrevLetter.NextWritePos + new Vector(0, RenderSize.Y);
            }
            else
            {
                if (MustScale)
                {
                    if (this.OwnWord.Prev != null && !Info.AllWhitespace) return false;
                    RenderSize = this.GetMaxRenderSize(PageSize);
                    StartPosition = (PageSize - RenderSize) / 2;
                }
                else this.StartPosition.Y = (PageSize.Y - Height) / 2;

                EndPosition = StartPosition + RenderSize;
                NextWritePos = MustScale ? new Vector(-1, PageSize.Y + 1) : new Vector(this.EndPosition.X - LineDist, 0);
            }

            return this.InsidePage(PageSize);
        }

        public Point GetStartPoint() => new(this.StartPosition.X, this.StartPosition.Y);

        public Point GetEndPoint() => new(this.EndPosition.X, this.EndPosition.Y);
        public Rect GetImageRect()
        {
            return new Rect(this.GetStartPoint(), this.GetEndPoint());
        }

        public Vector GetMaxRenderSize(Vector PageSize)
        {
            double PRatio = PageSize.X / PageSize.Y;
            double IRatio = Width / Height;
            return PRatio < IRatio ? new Vector(-PageSize.X, PageSize.X / IRatio) : new Vector(-PageSize.Y * IRatio, PageSize.Y);
        }

        public ImageSource GetImage() => Image;
    }
}
