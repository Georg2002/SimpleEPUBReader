using EPUBParser;
using SkiaSharp;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Point = System.Windows.Point;

namespace EPUBRenderer
{
    internal class ImageLetter : Letter
    {
        public SKImage Image;
        public bool Inline;
        public ImageLetter(byte[] imageData, bool Inline, WordInfo wordInfo) : base(wordInfo)
        {
            Type = LetterTypes.Image;
            this.Inline = Inline;
            this.IsWordEnd = true;

            if (imageData == null)
            {
                Logger.Report("image data a missing", LogType.Error);
                this.Image = null;
            }

            try
            {
                using (var memStream = new MemoryStream(imageData))
                {
                    this.Image = SKImage.FromBitmap(SKBitmap.Decode(SKCodec.Create(SKData.Create(memStream))));

                }
            }
            catch (Exception ex)
            {
                Logger.Report("image from couldn't be loaded", LogType.Error);
                Logger.Report(ex.Message, LogType.Error);
            }
        }

        private double Width;
        private double Height;

        public override bool Position(LetterPlacementInfo info)
        {
            var PageSize = info.PageSize;
            if (Image == null) Width = Height = 300;
            else
            {
                this.Width = this.Style.Width ?? this.Image.Width * Renderer.WindowScale;
                this.Height = this.Style.Height ?? this.Image.Height * Renderer.WindowScale;
                double ratio = this.Image.Height / this.Image.Width;
                if (this.Style.Width.HasValue) this.Height = ratio * this.Width;
                else if (this.Style.Height.HasValue) this.Width = this.Height / ratio;
            }

            this.FontSize = StandardFontSize;

            bool MustScale = PageSize.X < Width || PageSize.Y < Height;
            StartPosition = IsPageStart ? new Vector(PageSize.X, 0) : new Vector(this.PrevLetter.EndPosition.X, 0);
            Vector RenderSize = new Vector(-Width, Height);
            if (Inline)
            {
                var dist = this.GetNewLineDist();
                double Scale = dist <= Width ? dist / Width : 1;
                RenderSize *= Scale;
                StartPosition = IsPageStart ? StartPosition : this.PrevLetter.NextWritePos;
                if (info.State == PositionState.Newline)
                {
                    this.StartPosition.X -= dist;
                    this.StartPosition.Y = 0;
                    IsPageStart = true;
                }
                if (!IsPageStart && this.PrevLetter.Type == LetterTypes.Letter)
                {
                    var txtPrev = PrevLetter as TextLetter;
                    this.FontSize = txtPrev.IsRuby ? txtPrev.OwnWord.Prev.Letters.Last().FontSize : txtPrev.FontSize;
                }
                StartPosition += new Vector(-(this.FontSize + RenderSize.X) / 2, 0);
                EndPosition = StartPosition + RenderSize;
                NextWritePos = IsPageStart ? new Vector(this.StartPosition.X + (this.FontSize + RenderSize.X) / 2, this.EndPosition.Y) : this.PrevLetter.NextWritePos + new Vector(0, RenderSize.Y);
            }
            else
            {

                if (MustScale)
                {
                    if (this.OwnWord.Prev != null && !info.AllWhitespace) return false;
                    RenderSize = this.GetMaxRenderSize(PageSize);
                    StartPosition = (PageSize - RenderSize) / 2;
                }
                else this.StartPosition.Y = (PageSize.Y - Height) / 2;

                EndPosition = StartPosition + RenderSize;
                NextWritePos = MustScale ? new Vector(-1, PageSize.Y + 1) : new Vector(0, PageSize.Y + 1);//force new page or force new line
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

        public SKImage GetImage() => Image;
    }
}
