using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
   public struct PosDef
    {
        public int FileNumber;
        public int Line;
        public int Word;
        public int Letter;


        public PosDef(int FileNumber = 1, int Line = 1, int Word = 1, int Letter = 1)
        {
            this.FileNumber = FileNumber;
            this.Line = Line;
            this.Word = Word;
            this.Letter = Letter;
        }
    }
}
