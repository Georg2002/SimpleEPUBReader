using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using JapaneseDictionary;
using System.Runtime.InteropServices;

namespace EPUBRenderer
{
    internal class TextLetter : Letter
    {
        public FontWeight Weight;
        public char Character;

        public Vector Offset;
        public float RelScale = 1;
        public bool Rotated => this.Rotation != 0;
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
            this.Character = character;
            Type = LetterTypes.Letter;
            var Style = wordInfo.Style;
            this.Weight = Style.Weight;
            this.OrigChar = this.Character;

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

        public override RectangleF GetMarkingRect() => new((float)EndPosition.X, (float)(StartPosition.Y - VertSpacing.Y), (float)FontSize, (float)(VertSpacing.Y * 2 + FontSize));

        private static Dictionary<int, IntPtr> FontCache = new(100);
        private int fontKey => ((UInt16)this.FontSize) ^ ((this.Style.Weight == FontWeights.Normal).GetHashCode() << 16);
        public override object GetRenderElement(Graphics graphics)
        {
            var key = this.fontKey;
            IntPtr res;
            if (FontCache.TryGetValue(key, out res)) return res;

            // = Win32Func.CreateFontA((int)(this.FontSize * 0.75f), 0, 0, (int)(this.Rotation * 10), 400, 0, 0, 0, 136, 8, 1, 5, 0, "Arial");

            graphics.ReleaseHdc();//unlock
            res = Marshal.AllocHGlobal(200);
            var logfont =  new LOGFONT();
            var font = new Font(CharInfo.StandardFontFamily, this.FontSize * 0.75f, this.Style.Weight == FontWeights.Normal ? System.Drawing.FontStyle.Regular : System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            font.ToLogFont(logfont);
            Marshal.StructureToPtr(logfont, res, false);
            graphics.GetHdc();
            FontCache[key] = res;
            return res;
        }

        public override string ToString() => Character.ToString();
    }
}
