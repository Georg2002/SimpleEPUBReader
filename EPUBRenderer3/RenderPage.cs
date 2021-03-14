using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
