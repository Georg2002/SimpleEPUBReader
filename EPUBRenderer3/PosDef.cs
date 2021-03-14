using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
   public struct PosDef
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
    }
}
