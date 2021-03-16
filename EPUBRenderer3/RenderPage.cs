using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer3
{
    internal class RenderPage
    {
        public List<Line> Lines;
        public PosDef StartPos;
        public PosDef EndPos;

        public override string ToString()
        {
            string Text = "";
            Lines.ForEach(a => Text = Text + a);
            return Text;
        }

        public RenderPage()
        {
            Lines = new List<Line>();
        }

        public bool Within(PosDef Pos)
        {
            return Pos >= StartPos && Pos <= EndPos;
        }

        internal PosDef Intersect(Point relPoint)
        {
            for (int Li = 0; Li < Lines.Count; Li++)
            {
                for (int W = 0; W < Lines[Li].Words.Count; W++)
                {
                    for (int Le = 0; Le < Lines[Li].Words[W].Letters.Count; Le++)
                    {
                        var Letter = Lines[Li].Words[W].Letters[Le];
                        if (Letter.Inside(relPoint))
                        {
                            return new PosDef(StartPos.FileIndex, Li, W, Le);
                        }
                    }
                }
            }
            return PosDef.InvalidPosition;
        }
    }
}
