using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace EPUBRenderer
{
    internal class ImageLetter : Letter
    {
        public ImageSource Image;
        public bool Inline;
        public override float FontSize => (float)Width;
        public ImageLetter(ImageSource Image, bool Inline, WordInfo wordInfo) : base(wordInfo)
        {
            Type = LetterTypes.Image;
            this.Inline = Inline;
            IsWordEnd = true;
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
                Width = Style.Width.HasValue ? Style.Width.Value : Image.Width;
                Height = Style.Height.HasValue ? Style.Height.Value : Image.Height;
                double ratio = Image.Height / Image.Width;
                if (Style.Width.HasValue) Height = ratio * Width;
                else if (Style.Height.HasValue) Width = Height / ratio;
            }

            bool MustScale = PageSize.X < Width || PageSize.Y < Height;
            StartPosition = IsPageStart ? new Vector(PageSize.X, 0) : new Vector(PrevLetter.EndPosition.X, 0);
            Vector RenderSize = new Vector(-Width, Height);
            if (Inline)
            {
                double Scale = LineDist <= Width ? LineDist / Width : 1;
                RenderSize *= Scale;
                StartPosition = IsPageStart ? StartPosition : PrevLetter.NextWritePos;
                if (Info.State == PositionState.Newline)
                {
                    StartPosition.X -= LineDist;
                    StartPosition.Y = 0;
                    IsPageStart = true;
                }
                float FS = StandardFontSize;
                if (!IsPageStart && PrevLetter.Type == LetterTypes.Letter) FS = ((TextLetter)PrevLetter).FontSize;
                StartPosition += new Vector(-(FS + RenderSize.X) / 2, 0);
                EndPosition = StartPosition + RenderSize;
                NextWritePos = IsPageStart ? new Vector(StartPosition.X + (FS + RenderSize.X) / 2, EndPosition.Y) : PrevLetter.NextWritePos + new Vector(0, RenderSize.Y);
            }
            else
            {
                if (MustScale)
                {
                    if (PrevWord != null && !Info.AllWhitespace) return false;
                    RenderSize = GetMaxRenderSize(PageSize);
                    StartPosition = (PageSize - RenderSize) / 2;
                }
                else StartPosition.Y = (PageSize.Y - Height) / 2;

                EndPosition = StartPosition + RenderSize;
                NextWritePos = MustScale ? new Vector(-1, PageSize.Y + 1) : new Vector(EndPosition.X - LineDist, 0);
            }

            return InsidePage(PageSize);
        }

        public Point GetStartPoint() => new(StartPosition.X, StartPosition.Y);
        public Point GetEndPoint() => new(EndPosition.X, EndPosition.Y);
        public Rect GetImageRect() => new(EndPosition.X, StartPosition.Y, StartPosition.X - EndPosition.X, EndPosition.Y - StartPosition.Y);

        public Vector GetMaxRenderSize(Vector PageSize)
        {
            double PRatio = PageSize.X / PageSize.Y;
            double IRatio = Width / Height;
            return PRatio < IRatio ? new Vector(-PageSize.X, PageSize.X / IRatio) : new Vector(-PageSize.Y * IRatio, PageSize.Y);
        }

        public override object GetRenderElement() => Image;
    }
}
