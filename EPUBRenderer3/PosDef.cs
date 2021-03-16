using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct PosDef
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        public int FileIndex;
        public int Line;
        public int Word;
        public int Letter;


        public PosDef(int FileIndex, int Line, int Word, int Letter)
        {
            this.FileIndex = FileIndex;
            this.Line = Line;
            this.Word = Word;
            this.Letter = Letter;
        }

        public static PosDef InvalidPosition = new PosDef(-1, -1, -1, -1);

        public static bool operator <(PosDef A, PosDef B)
        {
            if (A.FileIndex < B.FileIndex)
            {
                return true;
            }
            else if (A.FileIndex == B.FileIndex)
            {
                if (A.Line < B.Line)
                {
                    return true;
                }
                else if (A.Line == B.Line)
                {
                    if (A.Word < B.Word)
                    {
                        return true;
                    }
                    else if (A.Word == B.Word)
                    {
                        return A.Letter < B.Letter;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool operator >(PosDef A, PosDef B)
        {
            return !(A < B) && A != B;
        }

        public static bool operator <=(PosDef A, PosDef B)
        {
            return A < B || A == B;
        }

        public static bool operator >=(PosDef A, PosDef B)
        {
            return A > B || A == B;
        }

        public static bool operator ==(PosDef A, PosDef B)
        {
            return A.Line == B.Line && A.Word == B.Word && A.Letter == B.Letter;
        }

        public static bool operator !=(PosDef A, PosDef B)
        {
            return !(A == B);
        }

        public override string ToString()
        {
            return $"{FileIndex}|{Line}|{Word}|{Letter}";
        }

        internal void Increment(List<Line> lines)
        {
            if (Letter < lines[Line].Words[Word].Letters.Count - 1)
            {
                Letter++;
            }
            else
            {
                if (Word < lines[Line].Words.Count - 1)
                {
                    Word++;
                    Letter = 0;
                }
                else
                {
                    if (Line < lines.Count - 1)
                    {
                        Line++;
                        Word = 0;
                        Letter = 0;
                    }
                    else
                    {
                        Line = -1;
                        Word = -1;
                        Letter = -1;
                    }
                }
            }
        }
    }
}
