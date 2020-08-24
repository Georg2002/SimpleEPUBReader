using EPUBParser;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public List<Writing> TextParts;
        public List<ImageInText> Images;
        public List<Marking> Markings;
        private EpubSettings Settings;
        private bool Viewing = false;

        public PageRenderer(EpubSettings pageSettings, Vector PageSize)
        {
            this.PageSize = PageSize;
            this.Settings = pageSettings;
            TextParts = new List<Writing>();
            Images = new List<ImageInText>();
            Markings = new List<Marking>();
        }

        public void Load()
        {
            Viewing = true;
            InvalidateVisual();
        }

        public void Unload()
        {
            Viewing = false;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext Context)
        {
            Width = PageSize.X;
            Height = PageSize.Y;

            if (!Viewing)
                return;

            foreach (var TextPart in TextParts)
            {
                TextPart.RenderPosition = FlowDirectionModifiers.SetRenderPos(TextPart);
                FormattedText Text = GetText(TextPart);
                Context.DrawText(Text, new Point(TextPart.RenderPosition.X, TextPart.RenderPosition.Y));
            }

            foreach (var Image in Images)
            {
                if (Images.Count == 1 && TextParts.Count == 0)
                {
                    Vector Dimensions = new Vector(Image.Image.Width, Image.Image.Height);
                    double ImageRatio = Dimensions.X / Dimensions.Y;
                    double PageRatio = PageSize.X / PageSize.Y;
                    Vector ScaleSize;
                    Vector Start;
                    if (ImageRatio > PageRatio)
                    {
                        ScaleSize = new Vector(PageSize.X, PageSize.Y / ImageRatio * PageRatio);
                        Start = new Vector(0, (PageSize.Y - ScaleSize.Y) / 2);
                    }
                    else
                    {
                        ScaleSize = new Vector(PageSize.X * ImageRatio / PageRatio, PageSize.Y);
                        Start = new Vector((PageSize.X - ScaleSize.X) / 2, 0);
                    }
                    Context.DrawImage(Image.Image, new Rect(Start.X, Start.Y, ScaleSize.X, ScaleSize.Y));
                }
                else
                {
                    Context.DrawImage(Image.Image, Image.Rectangle);
                }
            }
            Context.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Red, 2), new Rect(0, 0, Width, Height));

            foreach (var Marking in Markings)
            {
                if (Marking.CharEndIndex >= TextParts.Count)
                {
                    Marking.CharEndIndex = TextParts.Count - 1;
                }
                if (Marking.CharStartIndex < 0)
                {
                    Marking.CharStartIndex = 0;
                }
                var WritingsBetween = TextParts.GetRange(Marking.CharStartIndex, 1 + Marking.CharEndIndex - Marking.CharStartIndex);
                foreach (var Writing in WritingsBetween)
                {
                    Vector Size = FlowDirectionModifiers.GetMarkingSize();
                    var FirstPoint = new Point(Writing.WritingPosition.X, Writing.RenderPosition.Y );
                    Vector MarkingOffset = FlowDirectionModifiers.GetMarkingOffset();
                    FirstPoint.Offset(MarkingOffset.X, MarkingOffset.Y);
                    var SecondPoint = FirstPoint;
                    SecondPoint.Offset(Size.X, Size.Y);
                    Context.DrawRectangle(Marking.Color, null, new Rect(FirstPoint, SecondPoint));                   
                }
            }            
        }

        private FormattedText GetText(Writing textPart)
        {
            var Text = new FormattedText(textPart.Text.ToString(),
                 CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                 Typeface, textPart.FontSize, Brushes.Black, 1);
            Text.TextAlignment = TextAlignment.Center;
            return Text;
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
            return FlowDirectionModifiers.InPage(AfterImagePos, PageSize);
        }

        internal static string GetRuby(TextLinePart textPart)
        {
            if (textPart.Type == LinePartTypes.sesame)
            {
                return new string('﹅', textPart.Text.Length);
            }
            else
            {
                return textPart.Ruby;
            }
        }

        internal bool InPage(Vector writingPosition)
        {
            return FlowDirectionModifiers.InPage(writingPosition, PageSize);
        }

        internal List<Writing> GetMainTextWritings(string word)
        {
            Vector WritingPosCopy = CurrentWritePos;
            List<Writing> Result = new List<Writing>();
            bool HadFix = false;

            if (word == "")
            {
                var Item = new Writing();
                Item.Text = ' ';
                Item.WritingPosition = WritingPosCopy;
                Item.FontSize = 1;
                Result.Add(Item);
                return Result;
            }

            foreach (char c in word)
            {
                double StartOffset = 0;
                var NewItem = new Writing();
                NewItem.FontSize = ChapterPagesCreator.FontSize;
                if (Settings.Vertical)
                {
                    if (GlobalSettings.VerticalVisualFixes.ContainsKey(c))
                    {
                        var Info = GlobalSettings.VerticalVisualFixes[c];
                        NewItem.Text = Info.Replacement;
                        StartOffset = Info.WriteOffsetStart * ChapterPagesCreator.FontSize;
                        HadFix = true;
                    }
                    else
                        NewItem.Text = c;
                }
                else
                { NewItem.Text = c; }

                if (HadFix)
                {
                    NewItem.WritingPosition.Y += StartOffset;
                    HadFix = false;
                }
                NewItem.WritingPosition += FlowDirectionModifiers.GetAfterWritingPosition(WritingPosCopy, NewItem);
                WritingPosCopy = NewItem.WritingPosition;
                Result.Add(NewItem);
            }
            if (FlowDirectionModifiers.NeedsToWrap(Result.Last().WritingPosition, PageSize))
            {
                Vector StartPos = Result.First().WritingPosition;
                Vector ReferenceOffset = FlowDirectionModifiers.NewLinePosition(StartPos, PageSize) - StartPos;
                if (FlowDirectionModifiers.NeedsToWrap(Result.Last().WritingPosition + ReferenceOffset, PageSize))
                {
                    ReferenceOffset = new Vector();
                    foreach (var item in Result)
                    {
                        var NewPos = item.WritingPosition + ReferenceOffset;
                        if (FlowDirectionModifiers.NeedsToWrap(NewPos, PageSize))
                        {
                            ReferenceOffset = FlowDirectionModifiers.NewLinePosition(NewPos, PageSize) - item.WritingPosition;
                            NewPos = item.WritingPosition + ReferenceOffset;
                        }
                        item.WritingPosition = NewPos;
                    }

                }
                else
                {
                    Result.ForEach(a => a.WritingPosition += ReferenceOffset);
                }

            }
            return Result;
        }

        public List<Writing> GetRubyWritings(string ruby, List<Writing> mainTextWritings)
        {
            List<Writing> Result = new List<Writing>();
            Vector StartPosition = mainTextWritings.First().WritingPosition;
            Vector FirstWritingLength = FlowDirectionModifiers.GetAfterWritingPosition(new Vector(), mainTextWritings.First());
            StartPosition -= FirstWritingLength;
            Vector EndPosition = mainTextWritings.Last().WritingPosition;
            Vector Length = EndPosition - StartPosition;
            Vector Offset = Length / ruby.Length;
            Offset = FlowDirectionModifiers.RubyMinimumDistance(Offset);
            Vector Middle = (StartPosition + EndPosition) / 2;
            Vector RubyWritePosition = Middle - Offset * (ruby.Length - 1) / 2 + FlowDirectionModifiers.GetRubyStartOffset();
            foreach (char c in ruby)
            {
                char Text;
                if (GlobalSettings.VerticalVisualFixes.ContainsKey(c))
                {
                    Text = GlobalSettings.VerticalVisualFixes[c].Replacement;
                }
                else
                {
                    Text = c;
                }

                var Writing = new Writing
                {
                    Text = Text,
                    FontSize = ChapterPagesCreator.RubyFontSize * ChapterPagesCreator.FontSize,
                    WritingPosition = RubyWritePosition
                };
                Result.Add(Writing);
                RubyWritePosition += Offset;
            }
            return Result;
        }

        internal void Write(List<Writing> writings)
        {
            TextParts.AddRange(writings);
        }

        public void AddImage(ImageLinePart part)
        {
            var Image = new ImageInText();
            Image.Image = part.GetImage();
            Vector Dimensions = new Vector(Image.Image.Width, Image.Image.Height);
            if (CurrentWritePos != FlowDirectionModifiers.GetStartWritingPosition(PageSize))
            {
                CurrentWritePos = FlowDirectionModifiers.NewLinePosition(CurrentWritePos, PageSize);
            }
            Image.Rectangle = FlowDirectionModifiers.GetImageRect(CurrentWritePos, PageSize, Dimensions);
            CurrentWritePos = FlowDirectionModifiers.GetAfterImagePosition(CurrentWritePos, PageSize, Dimensions);
            Images.Add(Image);
        }
    }


    public class ImageInText
    {
        public ImageSource Image;
        public Rect Rectangle;
    }

    public class Marking
    {
        public Marking() { }
        public Marking(int CharStartIndex, int CharEndIndex, Brush Color)
        {
            this.CharStartIndex = CharStartIndex;
            this.CharEndIndex = CharEndIndex;
            this.Color = Color;
        }

        public Brush Color;
        public int CharStartIndex;
        public int CharEndIndex;
    }

    public class Writing
    {
        public char Text;
        public double FontSize;
        public Vector RenderPosition;
        public Vector WritingPosition;
    }
}
