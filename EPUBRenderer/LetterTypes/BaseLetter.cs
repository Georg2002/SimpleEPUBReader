using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    internal enum LetterTypes
    {
        Letter, Image, Break, Marker
    }
    public enum PositionState
    {
        Normal, Newline, NormalAfterNewline, TightFit, Final
    }
    internal struct LetterPlacementInfo
    {
        public Vector PageSize;
        public PositionState State;
        public bool AllWhitespace;
    }
    internal struct WordInfo
    {
        internal WordStyle Style;
        internal bool IsRuby;
    }
    internal class Letter
    {
        public float FontSize;
        public const float StandardFontSize = 19;//19
        public const float RubyScale = 0.7f;//0.7
        public const float RubyFontSize = RubyScale * StandardFontSize;
        public const float LineDist = 1.1f * (StandardFontSize + RubyFontSize);
        public const float RubyOffset = 0.93f * LineDist;
        public static readonly Vector OutsideVector = new(-100000, -100000);
       
        internal bool IsRuby;
        internal bool IsWordEnd;
        internal bool IsPageStart;

        internal WordStyle Style;
        internal Word OwnWord;
        public Letter PrevLetter;
        public Letter(WordInfo wordInfo)
        {
            this.IsRuby = wordInfo.IsRuby;
            this.Style = wordInfo.Style;
        }
        public static float GetLineDist(float fontSize) => 1.1f * (fontSize + GetRubyFontSize(fontSize));
        public static float GetRubyFontSize(float fontSize) => RubyScale * fontSize;


        public bool DictSelected;
        public Vector StartPosition;
        public Vector EndPosition;
        public virtual Vector HitboxStart => this.StartPosition;
        public virtual Vector HitboxEnd => this.EndPosition;
        public Vector Middle => (this.HitboxStart + this.HitboxEnd) / 2;
        public Vector NextWritePos;
        public LetterTypes Type;
        public byte MarkingColorIndex;
        internal static Brush DictSelectionColor = new SolidColorBrush(new Color() { A = 100, B = 50, G = 50, R = 50 });

        public virtual bool Position(LetterPlacementInfo Info) => false;
        internal bool Inside(Point relPoint) => relPoint.X < this.HitboxStart.X && relPoint.Y > this.HitboxStart.Y && relPoint.X > this.HitboxEnd.X && relPoint.Y < this.HitboxEnd.Y;

        //arranged to avoid negative numbers
        public virtual Rect GetMarkingRect() => new(EndPosition.X, StartPosition.Y, StartPosition.X - EndPosition.X, EndPosition.Y - StartPosition.Y);
        public override string ToString() => Type.ToString();
        public bool InsidePageVert(Vector PageSize) => EndPosition.Y <= PageSize.Y;
        public bool InsidePageHor(Vector PageSize) => EndPosition.X >= 0;
        public bool InsidePage(Vector PageSize) => this.InsidePageHor(PageSize) && this.InsidePageVert(PageSize);

        public (Vector, Vector) GetNeutralStartingPosition(LetterPlacementInfo Info)
        {
            Vector StartPosition;
            Vector EndPosition;
            var PageSize = Info.PageSize;
            if (IsPageStart)
            {
                StartPosition = new Vector(PageSize.X - LineDist + StandardFontSize, 0);
                EndPosition = new Vector(PageSize.X - LineDist, 0);
            }
            else
            {
                StartPosition = PrevLetter.NextWritePos;
                EndPosition = StartPosition + new Vector(-StandardFontSize, 0);
            }

            return (StartPosition, EndPosition);
        }

        public float GetNewLineDist()
        {
            float maxSize = -1;
            Letter prev = this.PrevLetter;
            while (true)
            {
                if (prev == null) break;
                if (prev.Type == LetterTypes.Letter)
                {
                    var prevLetter = (TextLetter)prev;
                    if (prevLetter.StartPosition.X != this.StartPosition.X) break;
                    if (maxSize < prevLetter.FontSize) maxSize = prevLetter.FontSize;
                }
                prev = prev.PrevLetter;
            }
            if (maxSize < 0) maxSize = Letter.StandardFontSize;
            return GetLineDist(maxSize);
        }
    }
}
