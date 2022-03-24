using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EPUBRenderer3
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct PosDef
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        [XmlIgnore]
        public int FileIndex;
        [XmlIgnore]
        public int Word;
        [XmlIgnore]
        public int Letter;
        [XmlText]
        public string ShrtTxt
        {
            get => $"{FileIndex}|{Word }|{Letter}";
            set
            {
                var Numbers = value.Split('|');
                if (Numbers.Length != 3) return;
                try
                {
                    FileIndex = Convert.ToInt32(Numbers[0]);
                    Word = Convert.ToInt32(Numbers[1]);
                    Letter = Convert.ToInt32(Numbers[2]);
                }
                catch (Exception) { return; }
            }
        }

        public PosDef(int FileIndex, int Word, int Letter)
        {
            this.FileIndex = FileIndex;
            this.Word = Word;
            this.Letter = Letter;
        }

        public static PosDef InvalidPosition = new PosDef(-1, -1, -1);

        public static bool operator <(PosDef A, PosDef B)
        {
            if (A.FileIndex < B.FileIndex) return true;
            else if (A.FileIndex == B.FileIndex)
            {
                if (A.Word < B.Word) return true;
                else if (A.Word == B.Word) return A.Letter < B.Letter;
                else return false;
            }
            else return false;
        }

        public static bool operator >(PosDef A, PosDef B) => !(A < B) && A != B;

        public static bool operator <=(PosDef A, PosDef B) => A < B || A == B;

        public static bool operator >=(PosDef A, PosDef B) => A > B || A == B;

        public static bool operator ==(PosDef A, PosDef B) => A.FileIndex == B.FileIndex && A.Word == B.Word && A.Letter == B.Letter;

        public static bool operator !=(PosDef A, PosDef B) => !(A == B);

        public override string ToString() => $"{FileIndex}|{Word}|{Letter}";

        internal void Increment(List<Word> words)
        {
            if (this == InvalidPosition) return;
            if (Word < words.Count)
            {
                if (Letter < words[this.Word].Letters.Count - 1) Letter++;
                else
                {
                    if (Word < words.Count - 1)
                    {
                        Word++;
                        Letter = 0;
                    }
                    else
                    {
                        Word = -1;
                        Letter = -1;
                        FileIndex = -1;
                    }
                }
            }
            else
            {
                Word = -1;
                Letter = -1;
                FileIndex = -1;
            }
        }

        internal void Decrement(List<Word> words)
        {
            if (this == InvalidPosition) return;
            if (this.Letter > 0) Letter--;
            else
            {
                if (Word > 0)
                {
                    Word--;
                    Letter = words[this.Word].Letters.Count - 1;
                }
                else
                {
                    Word = PosDef.InvalidPosition.Word;
                    Letter = PosDef.InvalidPosition.Letter;
                    FileIndex = PosDef.InvalidPosition.FileIndex;
                }
            }
        }

        internal static bool IsInvalid(PosDef pos) => pos.FileIndex == -1 || pos.Letter == -1 || pos.Word == -1;
    }
}
