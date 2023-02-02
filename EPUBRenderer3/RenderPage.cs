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
        public PageFile Page;
        public PageExtractDef Extract;
        public PosDef StartPos => new PosDef(Page.Index, Extract.startWord, Extract.startLetter);
        public PosDef EndPos => new PosDef(Page.Index, Extract.endWord, Extract.endLetter);

        public override string ToString()
        {
            string Text = "";
            Words.ForEach(a => Text = Text + a);
            return Text;
        }

        internal bool IsSingleImage()
        {
            bool ImageFound = false;
            for (int W = 0; W < Words.Count; W++)
            {
                for (int Le = 0; Le < Words[W].Letters.Count; Le++)
                {
                    var Letter = Words[W].Letters[Le];
                    if (Letter.Type == LetterTypes.Letter) return false;
                    else if (Letter.Type == LetterTypes.Image)
                    {
                        if (ImageFound) return false;
                        ImageFound = true;
                    }
                }
            }
            return true;
        }

        public RenderPage() => Words = new List<Word>();

        public bool Within(PosDef Pos) => Pos >= StartPos && Pos <= EndPos;

        private Letter GetLocal(PosDef Local) => Words[Local.Word].Letters[Local.Letter];

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, List<Word> allWords)
        {
            PosDef Start = Pos;
            PosDef End = Pos;
            Start.FileIndex = End.FileIndex = StartPos.FileIndex;
            var Local = ToLocal(Pos);
            byte ColorIndex = GetLocal(Local).MarkingColorIndex;
            if (ColorIndex == 0) return new Tuple<PosDef, PosDef>(PosDef.InvalidPosition, PosDef.InvalidPosition);
            Letter Letter;
            do
            {
                Local.Decrement(Words);
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex != ColorIndex && Letter.Type != LetterTypes.Break) break;
                else Start.Decrement(allWords);
            }
            while (true);
            Local = ToLocal(Pos);
            do
            {
                Local.Increment(Words);
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex != ColorIndex && Letter.Type != LetterTypes.Break) break;
                else End.Increment(allWords);
            }
            while (true);
            return new Tuple<PosDef, PosDef>(Start, End);
        }

        private PosDef ToLocal(PosDef Global)
        {
            if (Global.FileIndex != StartPos.FileIndex || Global < StartPos || Global > EndPos) return PosDef.InvalidPosition;
            if (StartPos.Word == Global.Word) return new PosDef(StartPos.FileIndex, 0, Global.Letter - StartPos.Letter);
            else return new PosDef(StartPos.FileIndex, Global.Word - StartPos.Word, Global.Letter);
        }

        private PosDef ToGlobal(PosDef Local)
        {
            var Global = new PosDef();
            Global.FileIndex = StartPos.FileIndex;
            Global.Word = StartPos.Word + Local.Word;
            Global.Letter = Local.Word == 0 ? StartPos.Letter + Local.Letter : Local.Letter;
            return Global;
        }

        internal PosDef Intersect(Point relPoint)
        {
            for (int W = 0; W < Words.Count; W++)
            {
                for (int Le = 0; Le < Words[W].Letters.Count; Le++)
                {
                    var Letter = Words[W].Letters[Le];
                    if (Letter.Inside(relPoint)) return ToGlobal(new PosDef(StartPos.FileIndex, W, Le));
                }
            }
            return PosDef.InvalidPosition;
        }
    }
}
