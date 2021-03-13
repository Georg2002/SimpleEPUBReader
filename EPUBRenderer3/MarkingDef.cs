using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
    public struct MarkingDef
    {
        public PosDef Pos;
        public byte ColorIndex;

        public MarkingDef(PosDef Pos,byte ColorIndex)
        {
            this.Pos = Pos;
            this.ColorIndex = ColorIndex;
        }
    }
}
