using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EPUBRenderer3
{
    public struct PageExtractDef
    {
        public int startLetter;
        public int endLetter;
        public int length => endLetter - startLetter + 1;

        internal (PageExtractDef front, PageExtractDef rear) Split(int letterCount)
        {
            PageExtractDef front = new PageExtractDef();
            PageExtractDef rear = new PageExtractDef();
            front.startLetter = startLetter;
            front.endLetter = startLetter + letterCount;
            rear.startLetter = front.endLetter + 1;
            rear.endLetter = endLetter;
            return (front, rear);
        }
    }
    public struct PosDef
    {
        [XmlIgnore]
        public int FileIndex;
        [XmlIgnore]
        public int Letter;
        [XmlText]
        public string ShrtTxt
        {
            get => $"{FileIndex}|{Letter}";
            set
            {
                var Numbers = value.Split('|');
                if (Numbers.Length != 2) return;
                try
                {
                    FileIndex = Convert.ToInt32(Numbers[0]);
                    Letter = Convert.ToInt32(Numbers[1]);
                }
                catch (Exception) { return; }
            }
        }

        public PosDef(int FileIndex, int Letter)
        {
            this.FileIndex = FileIndex;
            this.Letter = Letter;
        }

        public static readonly PosDef InvalidPosition = new PosDef(-1, -1);

        public static bool operator <(PosDef A, PosDef B)
        {
            if (A.FileIndex < B.FileIndex) return true;
            else if (A.FileIndex == B.FileIndex) return A.Letter < B.Letter;
            else return false;
        }

        public static bool operator >(PosDef A, PosDef B) => !(A < B) && A != B;

        public static bool operator <=(PosDef A, PosDef B) => A < B || A == B;

        public static bool operator >=(PosDef A, PosDef B) => A > B || A == B;

        public static bool operator ==(PosDef A, PosDef B) => A.FileIndex == B.FileIndex && A.Letter == B.Letter;

        public static bool operator !=(PosDef A, PosDef B) => !(A == B);
        public override bool Equals(object obj)
        {
            var other = obj as PosDef?;
            if (other == null) return false;
            return other == this;
        }
        public override int GetHashCode() => (FileIndex << 21) | Letter;//100% unique up to 1024 pages and 1 million letters
        public override string ToString() => $"{FileIndex}|{Letter}";

        internal void Increment(int letterCount)
        {
            if (this == InvalidPosition) return;
            if (Letter < letterCount - 1) Letter++;
            else
            {
                Letter = -1;
                FileIndex = -1;
            }
        }

        internal void Decrement()
        {
            if (this == InvalidPosition) return;
            if (this.Letter > 0) Letter--;
            else
            {
                Letter = PosDef.InvalidPosition.Letter;
                FileIndex = PosDef.InvalidPosition.FileIndex;
            }
        }
        internal bool IsInvalid => this.FileIndex == -1 || this.Letter == -1;
    }
}
