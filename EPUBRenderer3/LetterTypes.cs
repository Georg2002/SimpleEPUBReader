using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
    internal enum LetterTypes
    {
        Letter, Image, Break
    }

    internal struct LetterPlacementInfo
    {
        public Word PrevWord;
        public Word NextWord;
        public Letter PrevLetter;
        public Word OwnWord;
        public Vector PageSize;
        public bool Last;
        public bool NewLine;
        public bool TightFit;
    }

    internal class Letter
    {
        public const float StandardFontSize = 19;
        public const float RubyScale = 0.7f;
        public const float RubyFontSize = RubyScale * StandardFontSize;
        public const float LineDist = 1.1f * (StandardFontSize + RubyFontSize);
        public const float RubyOffset = 0.93f * LineDist;
        public const float RubyVertOffset = (StandardFontSize - RubyFontSize) * 0.24f;
        public static readonly Vector OutsideVector = new Vector(-100000, -100000);

        public Vector StartPosition;
        public Vector EndPosition;
        public virtual Vector HitboxStart { get => StartPosition; }
        public virtual Vector HitboxEnd { get => EndPosition; }
        public Vector NextWritePos;
        public LetterTypes Type;
        public byte MarkingColorIndex;
        public virtual bool Position(LetterPlacementInfo Info)
        {
            return false;
        }

        internal bool Inside(Point relPoint)
        {
            return relPoint.X < HitboxStart.X && relPoint.Y > HitboxStart.Y && relPoint.X > HitboxEnd.X && relPoint.Y < HitboxEnd.Y;
        }

        public virtual object GetRenderElement()
        {
            return null;
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public bool InsidePageVert(Vector PageSize)
        {
            return EndPosition.Y <= PageSize.Y;
        }

        public bool InsidePageHor(Vector PageSize)
        {
            return EndPosition.X >= 0;
        }

        public bool InsidePage(Vector PageSize)
        {
            return InsidePageHor(PageSize) && InsidePageVert(PageSize);
        }
    }

    internal class TextLetter : Letter
    {
        public float RelScale = 1;
        public float FontSize;
        public char Character;
        public Vector Offset;
        private readonly static Typeface StandardTypeface = new Typeface(new FontFamily("Hiragino Sans GB W3"), FontStyles.Normal,
     FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));
        private static readonly Vector HitboxExpansion = new Vector((LineDist - StandardFontSize) / 2, 0);
        private Vector _HitboxStart;
        public override Vector HitboxStart { get => _HitboxStart; }
        private Vector _HitboxEnd;
        public override Vector HitboxEnd { get => _HitboxEnd; }

        public TextLetter(char Character)
        {
            this.Character = Character;
            Type = LetterTypes.Letter;

            if (CharInfo.SpecialCharacters.ContainsKey(Character))
            {
                var Info = CharInfo.SpecialCharacters[Character];
                Offset = Info.Offset;
                RelScale = Info.Scaling;
                this.Character = Info.Replacement;
            }
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;
            var TightFit = Info.TightFit;
            var NewLine = Info.NewLine;
            var OwnWord = Info.OwnWord;
            var NextWord = Info.NextWord;
            var PrevWord = Info.PrevWord;
            if (OwnWord.Type == WordTypes.Normal)
            {
                FontSize = StandardFontSize;
                StartPosition = PrevLetter == null ? new Vector(PageSize.X - LineDist, 0) : PrevLetter.NextWritePos;
                Vector VertSpacing = new Vector();
                if (NextWord != null && NextWord.Type == WordTypes.Ruby)
                {
                    float RubyCount = NextWord.Letters.Count;
                    float TextCount = OwnWord.Letters.Count;
                    VertSpacing.Y = Math.Max((RubyCount * RubyScale / TextCount - 1) * StandardFontSize / 2, 0);
                }

                StartPosition = NewLine ? new Vector(StartPosition.X - LineDist, 0) : StartPosition;
                StartPosition += VertSpacing;
                EndPosition = StartPosition + new Vector(-FontSize, FontSize);

                if (TightFit && EndPosition.Y > PageSize.Y)
                {
                    StartPosition.Y = 0;
                    StartPosition.X -= LineDist;
                    EndPosition = StartPosition + new Vector(-FontSize, FontSize);

                }
                NextWritePos = EndPosition + new Vector(FontSize, 0) + VertSpacing;
                _HitboxStart = StartPosition + HitboxExpansion - VertSpacing;
                _HitboxEnd = EndPosition - HitboxExpansion + VertSpacing;
                return InsidePageVert(PageSize);
            }
            else
            {
                FontSize = RubyFontSize;
                var Last = Info.Last;

                float RubyCount = OwnWord.Letters.Count;
                float TextCount = PrevWord.Letters.Count;
                Vector VertSpacing = new Vector();
                VertSpacing.Y = Math.Max((TextCount / RubyCount - RubyScale) * StandardFontSize / 2, 0);

                double TextLength = PrevWord.Length();
                double RubyLength = OwnWord.Letters.Count * (RubyFontSize + 2 * VertSpacing.Y);
                if (((TextLetter)PrevLetter).FontSize == StandardFontSize)
                {
                    StartPosition = PrevLetter.EndPosition + new Vector(RubyOffset, RubyVertOffset - 0.5 * (TextLength + RubyLength));
                }
                else
                {
                    StartPosition = PrevLetter.NextWritePos;
                }
                StartPosition += VertSpacing;
                EndPosition = StartPosition + new Vector(-FontSize, FontSize);
                if (Last)
                {
                    NextWritePos = PrevWord.Letters.Last().NextWritePos;
                }
                else
                {
                    NextWritePos = EndPosition + new Vector(FontSize, 0) + VertSpacing;
                }
                _HitboxStart = OutsideVector;
                _HitboxEnd = OutsideVector;
                return true;
            }
        }

        public override object GetRenderElement()
        {
            return new FormattedText(Character.ToString(), System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.RightToLeft, StandardTypeface, FontSize * RelScale, Brushes.Black, 1)
            { TextAlignment = TextAlignment.Center };
        }

        public override string ToString()
        {
            return Character.ToString();
        }
    }

    internal class ImageLetter : Letter
    {
        public ImageSource Image;

        public ImageLetter(ImageSource Image)
        {
            Type = LetterTypes.Image;
            this.Image = Image;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;
            var PrevWord = Info.PrevWord;

            bool MustScale = PageSize.X < Image.Width || PageSize.Y < Image.Height;
            bool Inline = Image.Width <= LineDist * 2 && Image.Height <= 2 * LineDist;
            StartPosition = PrevLetter == null ? new Vector(PageSize.X, 0) : new Vector(PrevLetter.EndPosition.X, 0);
            Vector RenderSize = new Vector(-Image.Width, Image.Height);
            if (Inline)
            {
                double Scale = LineDist <= Image.Width ? LineDist / Image.Width : 1;
                RenderSize *= Scale;
                StartPosition = PrevLetter == null ? StartPosition : PrevLetter.NextWritePos;
                if (Info.NewLine)
                {
                    StartPosition.X -= LineDist;
                    StartPosition.Y = 0;
                    PrevLetter = null;
                }
                StartPosition += new Vector(-(StandardFontSize + RenderSize.X) / 2, StandardFontSize * CharInfo.FontOffset);
                EndPosition = StartPosition + RenderSize;
                NextWritePos = PrevLetter == null ? new Vector(StartPosition.X + (StandardFontSize + RenderSize.X) / 2, EndPosition.Y) : PrevLetter.NextWritePos + new Vector(0, RenderSize.Y);
            }
            else
            {
                if (MustScale)
                {
                    if (PrevWord != null) return false;
                    RenderSize = GetMaxRenderSize(PageSize);

                    StartPosition = (PageSize - RenderSize) / 2;
                    NextWritePos = new Vector(-1, PageSize.Y + 1);
                }
                else
                {
                    StartPosition.Y = (PageSize.Y - Image.Height) / 2;
                }

                EndPosition = StartPosition + RenderSize;
                NextWritePos = new Vector(EndPosition.X - LineDist, 0);
            }

            return InsidePage(PageSize);
        }

        public Vector GetMaxRenderSize(Vector PageSize)
        {
            double PRatio = PageSize.X / PageSize.Y;
            double IRatio = Image.Width / Image.Height;
            return PRatio < IRatio ? new Vector(-PageSize.X, PageSize.X / IRatio) : new Vector(-PageSize.Y * IRatio, PageSize.Y);
        }

        public override object GetRenderElement()
        {
            return Image;
        }
    }

    internal class BreakLetter : Letter
    {
        public BreakLetter()
        {
            Type = LetterTypes.Break;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;

            if (PrevLetter == null)
            {
                StartPosition = new Vector(PageSize.X - LineDist + StandardFontSize, 0);
                EndPosition = new Vector(PageSize.X - LineDist, 0);
            }
            else
            {
                StartPosition = PrevLetter.NextWritePos;
                EndPosition = StartPosition + new Vector(-StandardFontSize, 0);
            }
            if (PrevLetter != null && PrevLetter.Type == LetterTypes.Image)
            {
                //ignores the first break after an image in order to remove redundant breaks
                NextWritePos = StartPosition;
            }
            else
            {
                NextWritePos = new Vector(StartPosition.X - LineDist, 0);
            }
            return true;
        }

        public override object GetRenderElement()
        {
            return null;
        }
    }
}
