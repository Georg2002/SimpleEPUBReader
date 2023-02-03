using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public IEnumerable<Letter> Content => Page.Content.Skip(Extract.startLetter).Take(Extract.length);
        public PosDef StartPos => new PosDef(Page.Index, Extract.startLetter);
        public PosDef EndPos => new PosDef(Page.Index, Extract.endLetter);
        public RenderPage(PageFile page) => this.Page = page;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Extract.length + 100);
            foreach (Letter letter in Content) sb.Append(letter.ToString());
            return sb.ToString();
        }

        internal bool IsSingleImage()
        {
            bool ImageFound = false;
            foreach (var letter in Content)
            {
                if (letter.Type == LetterTypes.Letter) return false;
                else if (letter.Type == LetterTypes.Image)
                {
                    if (ImageFound) return false;
                    ImageFound = true;
                }
            }
            return true;
        }
        public bool Within(PosDef Pos) => Pos >= StartPos && Pos <= EndPos;

        private Letter GetLocal(PosDef Local) => Page.Content[Local.Letter + Extract.startLetter];

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, List<Letter> allContent)
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
                Local.Decrement();
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex != ColorIndex && Letter.Type != LetterTypes.Break) break;
                else Start.Decrement();
            }
            while (true);
            Local = ToLocal(Pos);
            do
            {
                Local.Increment(Extract.length);
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex != ColorIndex && Letter.Type != LetterTypes.Break) break;
                else End.Increment(Extract.length);
            }
            while (true);
            return new Tuple<PosDef, PosDef>(Start, End);
        }

        private PosDef ToLocal(PosDef Global)
        {
            if (Global.FileIndex != StartPos.FileIndex || Global < StartPos || Global > EndPos) return PosDef.InvalidPosition;
            return new PosDef(StartPos.FileIndex, Global.Letter - StartPos.Letter);
        }
        private PosDef ToGlobal(PosDef Local) => new PosDef(Local.FileIndex, Local.Letter + StartPos.Letter);
        internal PosDef Intersect(Point relPoint)
        {
            var i = 0;
            foreach (var letter in Content)
            {
                if (letter.Inside(relPoint)) break;
                i++;
            }
            if (i == Extract.length) return PosDef.InvalidPosition;
            return ToGlobal(new PosDef(StartPos.FileIndex, i));
        }

        private LetterPlacementInfo Info = new LetterPlacementInfo();//less garbage collection

        public int Position(Letter prevLetter, Vector PageSize, bool NewLine = false, bool TightFit = false, bool FinalRound = false)
        {
            this.Info.PageSize = PageSize;
            this.Info.PrevLetter = prevLetter;
            this.Info.NewLine = NewLine;
            this.Info.TightFit = TightFit;


            int Fit = 0;
            bool AllFit = true;
            bool fitHorizontal = false;
            foreach (var letter in Content)
            {
                letter.PrevLetter = Info.PrevLetter;
                bool LetterFit = letter.Position(Info);
                Info.NewLine = false;
                Info.PrevLetter = letter;
                if (LetterFit) Fit++;
                else
                {
                    fitHorizontal = letter.PrevLetter.InsidePageHor(PageSize);
                    AllFit = false;
                    break;
                }
            }
            if (Fit != 0 && !fitHorizontal) return 0;
            if (FinalRound) return Fit;
            if (!AllFit)
            {
                if (NewLine) Fit = Position(prevLetter, PageSize, NewLine: false, TightFit: true);
                else
                {
                    if (TightFit) Fit = Position(prevLetter, PageSize, NewLine: false, TightFit: true, FinalRound: true);
                    else Fit = Position(prevLetter, PageSize, true);
                }
            }


            return Fit;
        }
    }
}
