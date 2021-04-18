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

        internal bool IsSingleImage()
        {

            for (int Li = 0; Li < Lines.Count; Li++)
            {
                for (int W = 0; W < Lines[Li].Words.Count; W++)
                {
                    for (int Le = 0; Le < Lines[Li].Words[W].Letters.Count; Le++)
                    {
                        var Letter = Lines[Li].Words[W].Letters[Le];
                        if (Letter.Type == LetterTypes.Letter)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public RenderPage()
        {
            Lines = new List<Line>();
        }

        public bool Within(PosDef Pos)
        {
            return Pos >= StartPos && Pos <= EndPos;
        }

        private Letter GetLocal(PosDef Local)
        {
            return Lines[Local.Line].Words[Local.Word].Letters[Local.Letter];
        }

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, List<Line> AllLines)
        {
            PosDef Start = Pos;
            PosDef End = Pos;
            Start.FileIndex = End.FileIndex = StartPos.FileIndex;
            var Local = ToLocal(Pos);
            byte ColorIndex = GetLocal(Local).MarkingColorIndex;
            if (ColorIndex == 0)
            {
                return new Tuple<PosDef, PosDef>(PosDef.InvalidPosition, PosDef.InvalidPosition);
            }
            Letter Letter;
            do
            {
                Local.Decrement(Lines);
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex != ColorIndex && Letter.Type != LetterTypes.Break)
                {
                    break;
                }
                else
                {
                    Start.Decrement(AllLines);
                }
            }
            while (true);
            Local = ToLocal(Pos);
            do
            {
                Local.Increment(Lines);
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex != ColorIndex && Letter.Type != LetterTypes.Break)
                {
                    break;
                }
                else
                {
                    End.Increment(AllLines);
                }
            }
            while (true);
            return new Tuple<PosDef, PosDef>(Start, End);
        }

        private PosDef ToLocal(PosDef Global)
        {
            if (Global.FileIndex != StartPos.FileIndex || Global < StartPos || Global > EndPos) return PosDef.InvalidPosition;

            if (StartPos.Line == Global.Line)
            {
                if (StartPos.Word == Global.Word)
                {
                    return new PosDef(StartPos.FileIndex, 0, 0, Global.Letter - StartPos.Letter);
                }
                else
                {
                    return new PosDef(StartPos.FileIndex, 0, Global.Word - StartPos.Word, Global.Letter);
                }
            }
            else
            {
                return new PosDef(StartPos.FileIndex, Global.Line - StartPos.Line, Global.Word, Global.Letter);
            }
        }

        private PosDef ToGlobal(PosDef Local)
        {
            var Global = new PosDef();
            Global.FileIndex = StartPos.FileIndex;
            Global.Line = StartPos.Line + Local.Line;
            if (Local.Line == 0)
            {
                Global.Word = StartPos.Word + Local.Word;
                Global.Letter = Local.Word == 0 ? StartPos.Letter + Local.Letter : Local.Letter;
            }
            else
            {
                Global.Word = Local.Word;
                Global.Letter = Local.Letter;
            }
            return Global;
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
                            return ToGlobal(new PosDef(StartPos.FileIndex, Li, W, Le));
                        }
                    }
                }
            }
            return PosDef.InvalidPosition;
        }
    }
}
