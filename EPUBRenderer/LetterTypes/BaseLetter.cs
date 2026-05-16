using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    internal enum LetterTypes : sbyte
    {
        Letter, Image, Break, Marker
    }
    public enum PositionState : sbyte
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
        public const double StandardLineDist = StandardFontSize * 1.1 * (1.0 + RubyScale);
        public const float RubyScale = 0.7f;//0.7
        public double LineDist => 1.1 * (1.0 + RubyScale) * (this.IsRuby ? this.PrevLetter.OwnWord.Letters.Last().FontSize : this.FontSize);
        public double RubyOffset => 0.93 * this.LineDist;
        public static readonly Vector OutsideVector = new(-100000, -100000);

        public bool DictSelected;
        internal bool IsRuby;
        internal bool IsWordEnd;
        internal bool IsPageStart;
        public byte MarkingColorIndex;
        internal readonly static SKPaint DictSelectionPaint;

        internal WordStyle Style;
        internal Word OwnWord;
        public Letter PrevLetter;
        public Letter(WordInfo wordInfo)
        {
            this.IsRuby = wordInfo.IsRuby;
            this.Style = wordInfo.Style;
        }
        /*     public static float GetLineDist(float fontSize) => 1.1f * (fontSize + GetRubyFontSize(fontSize));
             public static float GetRubyFontSize(float fontSize) => RubyScale * fontSize;
        */


        public Vector StartPosition;
        public Vector EndPosition;
        public virtual Vector HitboxStart => this.StartPosition;
        public virtual Vector HitboxEnd => this.EndPosition;
        public Vector Middle => (this.HitboxStart + this.HitboxEnd) / 2;
        public Vector NextWritePos;
        public LetterTypes Type;
        static Letter()
        {
            DictSelectionPaint = new SKPaint { Color = new SKColor(50, 50, 50, 100), IsAntialias = true, Style = SKPaintStyle.Fill };
        }
        public virtual bool Position(LetterPlacementInfo Info) => false;
        internal bool Inside(Point relPoint) => !this.IsRuby && relPoint.X <= this.HitboxStart.X && relPoint.Y >= this.HitboxStart.Y && relPoint.X >= this.HitboxEnd.X && relPoint.Y <= this.HitboxEnd.Y;

        //arranged to avoid negative numbers
        public virtual Rect GetMarkingRect() => new(EndPosition.X, StartPosition.Y, StartPosition.X - EndPosition.X, EndPosition.Y - StartPosition.Y);
        public override string ToString() => Type.ToString();
        public bool InsidePageVert(Vector PageSize) => EndPosition.Y <= PageSize.Y;
#pragma warning disable IDE0060 // Remove unused parameter
        public bool InsidePageHor(Vector PageSize) => EndPosition.X >= 0;
#pragma warning restore IDE0060 // Remove unused parameter
        public bool InsidePage(Vector PageSize) => this.InsidePageHor(PageSize) && this.InsidePageVert(PageSize);

      
        public double GetNewLineDist()
        {
            double maxDist = -1;
            Letter l = this;

            while (true)
            {
                if (l == null) break;
                if (l.Type == LetterTypes.Letter && !l.IsRuby)
                {
                    var txtLetter = (TextLetter)l;
                    if (txtLetter.StartPosition.X != this.StartPosition.X) break;
                    if (maxDist < txtLetter.LineDist) maxDist = txtLetter.LineDist;
                }
                l = l.PrevLetter;
            }
            if (maxDist < 0) maxDist = Letter.StandardFontSize;
            return maxDist;
        }
    }
}
