using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WatconWrapper;

namespace EPUBRenderer3
{
    internal class TextLetter : Letter
    {
        public float RelScale = 1;
        public float FontSize;
        public FontWeight Weight;
        public Typeface Typeface;
        public char Character;
        public Vector Offset;
        public bool Rotated => this.Rotation != 0;
        public double Rotation = 0;

        private static readonly Vector HitboxExpansion = new Vector((LineDist - StandardFontSize) / 2, 0);
        private Vector _HitboxStart;
        public override Vector HitboxStart  => _HitboxStart; 
        private Vector _HitboxEnd;
        public override Vector HitboxEnd=> _HitboxEnd; 
        private Vector VertSpacing;
        internal bool IsRuby;

        public TextLetter(char character, WordStyle Style)
        {
            this.Character = character;
            Type = LetterTypes.Letter;
            this.Weight = Style.Weight;
            Typeface = new Typeface(new FontFamily("Hiragino Sans GB"), FontStyles.Normal,
            Weight, new FontStretch(), new FontFamily("Global User Interface"));

            if (CharInfo.SpecialCharacters.ContainsKey(this.Character))
            {
                var Info = CharInfo.SpecialCharacters[this.Character];
                Offset = Info.Offset;
                RelScale = Info.Scaling;
                this.Rotation = Info.Rotation;
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
            var Style = OwnWord.Style;

            if (OwnWord.Type == WordTypes.Normal)
            {
                FontSize = StandardFontSize * Style.RelativeFontSize;
                StartPosition = PrevLetter == null ? new Vector(PageSize.X - LineDist, 0) : PrevLetter.NextWritePos;
                VertSpacing = new Vector();
                if (NextWord != null && NextWord.Type == WordTypes.Ruby)
                {
                    float RubyCount = NextWord.Letters.Count;
                    float TextCount = OwnWord.Letters.Count;
                    VertSpacing.Y = Math.Max((RubyCount * RubyScale / TextCount - 1) * StandardFontSize / 2, 0);
                }

                StartPosition = NewLine ? new Vector(StartPosition.X - GetNewLineDist(), 0) : StartPosition;
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
                IsRuby = true;
                var MainWordFontSize = ((TextLetter)PrevWord.Letters.Last()).FontSize;
                FontSize = RubyFontSize * Style.RelativeFontSize;
                var Last = Info.Last;
                float RubyCount = OwnWord.Letters.Count;
                float TextCount = PrevWord.Letters.Count;
                VertSpacing = new Vector();
                VertSpacing.Y = Math.Max((TextCount / RubyCount - RubyScale) * MainWordFontSize / 2, 0);

                double TextLength = PrevWord.Length();
                double RubyLength = OwnWord.Letters.Count * (RubyFontSize * Style.RelativeFontSize + 2 * VertSpacing.Y);
                if (!((TextLetter)PrevLetter).IsRuby) StartPosition = PrevLetter.EndPosition + new Vector(RubyOffset * Style.RelativeFontSize, -0.5 * (TextLength + RubyLength));  
                else StartPosition = PrevLetter.NextWritePos;
                StartPosition += VertSpacing;
                EndPosition = StartPosition + new Vector(-FontSize, FontSize);
                if (Last) NextWritePos = PrevWord.Letters.Last().NextWritePos;
                else NextWritePos = EndPosition + new Vector(FontSize, 0) + VertSpacing;
                _HitboxStart = OutsideVector;
                _HitboxEnd = OutsideVector;
                return true;
            }
        }

        public override Rect GetMarkingRect() => new Rect
            {
                Y = StartPosition.Y - VertSpacing.Y,
                X = EndPosition.X,
                Width = FontSize,
                Height = VertSpacing.Y* 2 + FontSize
    };        

        public override object GetRenderElement(bool KatakanaLearningMode)
        {
            string DisplayedText = KatakanaLearningMode ? LanguageResources.SwitchKana(Character.ToString()) : Character.ToString();
            return new FormattedText(DisplayedText, System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.RightToLeft, Typeface, FontSize * RelScale, Brushes.Black, 1)
            { TextAlignment = TextAlignment.Center };
        }

        public override string ToString() => Character.ToString();    
    }
}
