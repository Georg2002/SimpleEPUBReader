using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using JapaneseDictionary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Documents;
using EPUBRenderer;

namespace EPUBRenderer
{
    internal class TextLetter : Letter
    {
        public FontWeight Weight;
        public Typeface Typeface;
        public char Character;

        public Vector Offset;
        public float RelScale = 1;
        public bool Rotated => Rotation != 0;
        public double Rotation = 0;

        public char OrigChar;

        private static readonly Vector HitboxExpansion = new((LineDist - StandardFontSize) / 2, 0);
        private Vector _HitboxStart;
        public override Vector HitboxStart => _HitboxStart;
        private Vector _HitboxEnd;
        public override Vector HitboxEnd => _HitboxEnd;
        private Vector VertSpacing;

        public TextLetter(char character, WordInfo wordInfo) : base(wordInfo)
        {
            Character = character;
            Type = LetterTypes.Letter;
            var Style = wordInfo.Style;
            Weight = Style.Weight;
            Typeface = Style.Typeface;
            OrigChar = Character;

            if (CharInfo.SpecialCharacters.ContainsKey(Character))
            {
                var Info = CharInfo.SpecialCharacters[Character];
                Offset = Info.Offset;
                RelScale = Info.Scaling;
                Rotation = Info.Rotation;
                Character = Info.Replacement;
            }
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PageSize = Info.PageSize;
            var TightFit = Info.State == PositionState.TightFit;
            var NewLine = Info.State == PositionState.Newline;

            if (IsRuby)
            {
                var MainWordFontSize = PrevWord.Letters.Last().FontSize;
                FontSize = RubyFontSize * Style.RelativeFontSize;
                float RubyCount = OwnWord.LetterCount;
                float TextCount = PrevWord.LetterCount;
                VertSpacing = new Vector();
                VertSpacing.Y = Math.Max((TextCount / RubyCount - RubyScale) * MainWordFontSize / 2, 0);

                double TextLength = PrevWord.Length();
                double RubyLength = OwnWord.LetterCount * (RubyFontSize * Style.RelativeFontSize + 2 * VertSpacing.Y);
                if (!PrevLetter.IsRuby) StartPosition = PrevLetter.EndPosition + new Vector(RubyOffset * Style.RelativeFontSize, -0.5 * (TextLength + RubyLength));
                else StartPosition = PrevLetter.NextWritePos;
                StartPosition += VertSpacing;
                EndPosition = StartPosition + new Vector(-FontSize, FontSize);
                if (IsWordEnd) NextWritePos = PrevWord.Letters.Last().NextWritePos;
                else NextWritePos = EndPosition + new Vector(FontSize, 0) + VertSpacing;
                _HitboxStart = OutsideVector;
                _HitboxEnd = OutsideVector;
                return true;
            }
            else
            {
                FontSize = StandardFontSize * Style.RelativeFontSize;
                StartPosition = IsPageStart ? new Vector(PageSize.X - LineDist, 0) : PrevLetter.NextWritePos;
                VertSpacing = new Vector();
                if (NextWord != null && NextWord.Type == WordTypes.Ruby)
                {
                    float RubyCount = NextWord.LetterCount;
                    float TextCount = OwnWord.LetterCount;
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
        }

        public override Rect GetMarkingRect() => new(EndPosition.X, StartPosition.Y - VertSpacing.Y, FontSize, VertSpacing.Y * 2 + FontSize);

        /*
     public override object GetRenderElement()
        {
            typefaceRun temp;
            GlyphRun run;
            GlyphTypeface gtypeface;
            var key = fontKey;
            if (!RunCache.TryGetValue(key, out temp))
            {
                Typeface.TryGetGlyphTypeface(out gtypeface);

                ushort gindex = 0;//default value
                var widths = new List<double> { gtypeface.AdvanceWidths[gindex] };
                run = new GlyphRun(gtypeface, 1, false, FontSize, 0.5f, new List<ushort> { gindex }, new Point(), widths, new List<Point> { new Point() }, new List<char> { Character }, null, new List<ushort> { 0 }, new List<bool> { false, false }, System.Windows.Markup.XmlLanguage.Empty);

                RunCache[fontKey] = temp = new typefaceRun() { run = run, typeface = gtypeface };

                var caretStops = run.CaretStops as List<bool>;
                caretStops.Clear();
                caretStops.Add(false);
                run.GlyphIndices.Clear();
                run.GlyphOffsets.Clear();
                run.ClusterMap.Clear();
                run.Characters.Clear();
                run.AdvanceWidths.Clear();
                //reset to workable state
            }

            run = temp.run;
            gtypeface = temp.typeface;
            addGlyph(run, gtypeface, Character);
            return run;
        }
        */

        public override string ToString() => Character.ToString();
    }
}
