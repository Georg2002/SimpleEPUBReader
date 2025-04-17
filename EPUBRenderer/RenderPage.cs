using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
{

    internal class RenderPage
    {
        public PageFile Page;
        public PageExtractDef Extract;
        public IEnumerable<Letter> Content => Page.Content.Skip(Extract.startLetter).Take(Extract.Length);
        public PosDef StartPos => new(Page.Index, Extract.startLetter);
        public PosDef EndPos => new(Page.Index, Extract.endLetter);
        public RenderPage(PageFile page) => this.Page = page;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Extract.Length + 100);
            foreach (Letter letter in this.Content) sb.Append(letter.ToString());
            return sb.ToString();
        }

        internal bool IsSingleImage()
        {
            bool ImageFound = false;
            foreach (var letter in this.Content)
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
        public bool Within(PosDef Pos) => Pos >= this.StartPos && Pos <= this.EndPos;

        private Letter GetLocal(PosDef Local) => Page.Content[Local.Letter + Extract.startLetter];

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, List<Letter> allContent)
        {
            PosDef Start = Pos;
            PosDef End = Pos;
            Start.FileIndex = End.FileIndex = this.StartPos.FileIndex;
            var Local = this.ToLocal(Pos);
            byte ColorIndex = this.GetLocal(Local).MarkingColorIndex;
            if (ColorIndex == 0) return new Tuple<PosDef, PosDef>(PosDef.InvalidPosition, PosDef.InvalidPosition);
            Letter Letter;
            do
            {
                Local.Decrement();
                if (Local.FileIndex == -1) break;
                Letter = this.GetLocal(Local);
                if (Letter.MarkingColorIndex == ColorIndex || Letter.Type == LetterTypes.Break) Start.Decrement();
                else break;
            }
            while (true);
            Local = this.ToLocal(Pos);
            do
            {
                Local.Increment(Extract.Length);
                if (Local.FileIndex == -1) break;
                Letter = this.GetLocal(Local);
                if (Letter.MarkingColorIndex == ColorIndex || Letter.Type == LetterTypes.Break) End.Increment(allContent.Count); 
                else break;
            }
            while (true);
            return new Tuple<PosDef, PosDef>(Start, End);
        }

        private PosDef ToLocal(PosDef Global)
        {
            if (Global.FileIndex != this.StartPos.FileIndex || Global < this.StartPos || Global > this.EndPos) return PosDef.InvalidPosition;
            return new PosDef(this.StartPos.FileIndex, Global.Letter - this.StartPos.Letter);
        }
        private PosDef ToGlobal(PosDef Local) => new(Local.FileIndex, Local.Letter + this.StartPos.Letter);
        internal PosDef Intersect(Point relPoint)
        {
            var i = 0;
            foreach (var letter in this.Content)
            {
                if (letter.Inside(relPoint)) break;
                i++;
            }
            if (i == Extract.Length) return PosDef.InvalidPosition;
            return this.ToGlobal(new PosDef(this.StartPos.FileIndex, i));
        }

        private LetterPlacementInfo Info = new();//less garbage collection
        public int Position(Vector PageSize)
        {
            this.Info.PageSize = PageSize;
            this.Info.State = PositionState.Normal;
            int FitCount = 0;
            Info.AllWhitespace = true;

            for (int i = 0; i < this.Content.Count(); i++)
            {
                var letter = this.Content.ElementAt(i);
                letter.IsPageStart = i == 0;
                bool LetterFit = letter.Position(Info);
                if (Info.State == PositionState.Newline) Info.State++;//only one newline
                if (!LetterFit)
                {
                    Info.State++;
                    i = FitCount - 1;//++ right after
                    if (Info.State == PositionState.Final) return FitCount;
                    continue;
                }
                if (letter.IsWordEnd)
                {
                    if (!letter.InsidePageHor(PageSize)) return FitCount;
                    FitCount = i + 1;//only set when full word has been written, only increased after
                    Info.State = PositionState.Normal;//reset
                }
                if (letter.Type == LetterTypes.Image || letter.Type == LetterTypes.Letter) Info.AllWhitespace = false;
            }
            return FitCount;
        }
    }
}
