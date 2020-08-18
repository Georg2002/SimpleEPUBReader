


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer
{
    public struct ChapterPosition
    {
        public int LineIndex;
        public int PartIndex;
        public int CharIndex;

        public ChapterPosition(int Line, int Part, int Char)
        {
            LineIndex = Line;
            PartIndex = Part;
            CharIndex = Char;
        }

        public override int GetHashCode()
        {
            return LineIndex * PartIndex * CharIndex;
        }

        public static bool operator <(ChapterPosition a, ChapterPosition b)
        {
            if (a.LineIndex < b.LineIndex)
            {
                return true;
            }
            else if (a.LineIndex == b.LineIndex && a.PartIndex < b.PartIndex)
            {
                return true;
            }
            else if (a.LineIndex == b.LineIndex && a.PartIndex == b.PartIndex && a.CharIndex < b.CharIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ChapterPosition))
            {
                return this == (ChapterPosition)obj;
            }
            else
            {
                return false;
            }

        }

        public static bool operator >(ChapterPosition a, ChapterPosition b)
        {
            return !(a == b || a < b);
        }

        public static bool operator !=(ChapterPosition a, ChapterPosition b)
        {
            return !(a == b);
        }

        public static bool operator ==(ChapterPosition a, ChapterPosition b)
        {
            return a.LineIndex == b.LineIndex && a.PartIndex == b.PartIndex && a.CharIndex == b.CharIndex;
        }
    }
}
