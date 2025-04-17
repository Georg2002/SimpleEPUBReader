using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using JapaneseDictionary;

namespace EPUBRenderer
{
    internal class TextLetter : Letter
    {
        private GlyphTypeface Typeface => PageFile.Typefaces[this.Style.Weight].Item1;
        private GlyphTypeface BackupTypeface => PageFile.Typefaces[this.Style.Weight].Item2;

        public char Character;

        public Vector Offset;
        public bool Rotated => this.Rotation != 0;
        public double Rotation;
        public float RelScale = 1;
        public char OrigChar;

        private static readonly Vector HitboxExpansion = new((LineDist - StandardFontSize) / 2, 0);
        private Vector _HitboxStart;
        public override Vector HitboxStart => _HitboxStart;
        private Vector _HitboxEnd;
        public override Vector HitboxEnd => _HitboxEnd;
        private Vector VertSpacing;

        public TextLetter(char character, WordInfo wordInfo) : base(wordInfo)
        {
            this.Character = character;
            Type = LetterTypes.Letter;
            this.OrigChar = this.Character;

            if (CharInfo.SpecialCharacters.ContainsKey(this.Character))
            {
                var info = CharInfo.SpecialCharacters[this.Character];
                Offset = info.Offset;
                RelScale = info.Scaling;
                this.Rotation = info.Rotation;
                this.Character = info.Replacement;
            }
        }
        public override bool Position(LetterPlacementInfo Info)
        {
            var PageSize = Info.PageSize;
            var TightFit = Info.State == PositionState.TightFit;
            var NewLine = Info.State == PositionState.Newline;

            if (IsRuby)
            {         
                var MainWordFontSize = this.OwnWord.Prev.Letters.Last().FontSize;
                this.FontSize = RubyFontSize * Style.RelativeFontSize;
                float RubyCount = OwnWord.LetterCount;
                float TextCount = this.OwnWord.Prev.LetterCount;
                VertSpacing = new Vector();
                VertSpacing.Y = Math.Max((TextCount / RubyCount - RubyScale) * MainWordFontSize / 2, 0);

                double TextLength = this.OwnWord.Prev.Length();
                double RubyLength = OwnWord.LetterCount * (RubyFontSize * Style.RelativeFontSize + 2 * VertSpacing.Y);
                if (!PrevLetter.IsRuby) StartPosition = PrevLetter.EndPosition + new Vector(RubyOffset * Style.RelativeFontSize, -0.5 * (TextLength + RubyLength));
                else StartPosition = PrevLetter.NextWritePos;
                StartPosition += VertSpacing;
                EndPosition = StartPosition + new Vector(-this.FontSize, this.FontSize);
                if (IsWordEnd) NextWritePos = this.OwnWord.Prev.Letters.Last().NextWritePos;
                else NextWritePos = EndPosition + new Vector(this.FontSize, 0) + VertSpacing;
                _HitboxStart = OutsideVector;
                _HitboxEnd = OutsideVector;
                return true;
            }
            else
            {
                this.FontSize = StandardFontSize * Style.RelativeFontSize;
                StartPosition = IsPageStart ? new Vector(PageSize.X - LineDist, 0) : PrevLetter.NextWritePos;
                VertSpacing = new Vector();
                if (this.OwnWord.Next != null && this.OwnWord.Next.Type == WordTypes.Ruby)
                {
                    float RubyCount = this.OwnWord.Next.LetterCount;
                    float TextCount = OwnWord.LetterCount;
                    VertSpacing.Y = Math.Max((RubyCount * RubyScale / TextCount - 1) * StandardFontSize / 2, 0);
                }

                StartPosition = NewLine ? new Vector(StartPosition.X - this.GetNewLineDist(), 0) : StartPosition;
                StartPosition += VertSpacing;
                EndPosition = StartPosition + new Vector(-this.FontSize, this.FontSize);

                if (TightFit && EndPosition.Y > PageSize.Y)
                {
                    StartPosition.Y = 0;
                    StartPosition.X -= LineDist;
                    EndPosition = StartPosition + new Vector(-this.FontSize, this.FontSize);
                }
                NextWritePos = EndPosition + new Vector(this.FontSize, 0) + VertSpacing;
                _HitboxStart = StartPosition + HitboxExpansion - VertSpacing;
                _HitboxEnd = EndPosition - HitboxExpansion + VertSpacing;
                return this.InsidePageVert(PageSize);
            }
        }

        public override Rect GetMarkingRect() => new()
        {
            Y = this.StartPosition.Y - this.VertSpacing.Y,
            X = EndPosition.X,
            Width = this.FontSize,
            Height = this.VertSpacing.Y * 2 + this.FontSize
        };
        public Tuple<GlyphTypeface, ushort> GetRenderingInfo()
        {
            var usedTf = this.Typeface;
            if (!this.Typeface.CharacterToGlyphMap.TryGetValue(this.Character, out var glyphIndex))
            {
                usedTf = this.BackupTypeface;
                if (!this.BackupTypeface.CharacterToGlyphMap.TryGetValue(this.Character, out glyphIndex))
                {
                    glyphIndex = this.Typeface.CharacterToGlyphMap['X'];
                    usedTf = this.Typeface;
                }
            }
            return new(usedTf, glyphIndex);

        }
        public GlyphRun CreateGlyphRun(Point baselineOrigin)
        {

            var glyphIndices = new ushort[1];
            var advanceWidths = new double[1];

            var usedTf = this.Typeface;

            if (!this.Typeface.CharacterToGlyphMap.TryGetValue(this.Character, out var glyphIndex))
            {
                usedTf = this.BackupTypeface;
                if (!this.BackupTypeface.CharacterToGlyphMap.TryGetValue(this.Character, out glyphIndex))
                {
                    glyphIndex = this.Typeface.CharacterToGlyphMap['X'];
                    usedTf = this.Typeface;
                }
            }
            glyphIndices[0] = glyphIndex;
            advanceWidths[0] = this.Typeface.AdvanceWidths[glyphIndex] * this.FontSize * this.RelScale;

            return new GlyphRun(
        usedTf, 0,false, this.FontSize * RelScale, 1,
        glyphIndices, baselineOrigin, advanceWidths,
        null, null, null, null, null, null);
        }
        public override string ToString() => Character.ToString();
    }
}
