using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
    internal enum LetterTypes
    {
        Letter, Image, Break, Marker
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
        public const float StandardFontSize = 19;//19
        public const float RubyScale = 0.7f;//0.7
        public const float RubyFontSize = RubyScale * StandardFontSize;
        public const float LineDist = 1.1f * (StandardFontSize + RubyFontSize);
        public const float RubyOffset = 0.93f * LineDist;
        public static readonly Vector OutsideVector = new Vector(-100000, -100000);
        public float GetLineDist(float fontSize) => 1.1f * (fontSize + GetRubyFontSize(fontSize));
        public float GetRubyFontSize(float fontSize) => RubyScale * fontSize;


        public bool DictSelected;
        public Vector StartPosition;
        public Vector EndPosition;
        public virtual Vector HitboxStart { get => StartPosition; }
        public virtual Vector HitboxEnd { get => EndPosition; }
        public Vector NextWritePos;
        public LetterTypes Type;
        public byte MarkingColorIndex;
        internal static Brush DictSelectionColor = new SolidColorBrush(new Color() { A = 100, B = 50, G = 50, R = 50 });
        public Letter PrevLetter;

        public virtual bool Position(LetterPlacementInfo Info)
        {
            return false;
        }

        internal bool Inside(Point relPoint)
        {
            return relPoint.X < HitboxStart.X && relPoint.Y > HitboxStart.Y && relPoint.X > HitboxEnd.X && relPoint.Y < HitboxEnd.Y;
        }

        public virtual object GetRenderElement(bool katakanaLearningMode)
        {
            return null;
        }

        public virtual Rect GetMarkingRect()
        {
            //arranged to avoid negative numbers
            return new Rect(EndPosition.X, StartPosition.Y, StartPosition.X - EndPosition.X, EndPosition.Y - StartPosition.Y);
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

        public (Vector, Vector) GetNeutralStartingPosition(LetterPlacementInfo Info)
        {
            Vector StartPosition;
            Vector EndPosition;
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
