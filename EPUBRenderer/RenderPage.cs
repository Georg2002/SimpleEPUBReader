using EPUBRenderer;
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
                if (Letter.MarkingColorIndex == ColorIndex || Letter.Type == LetterTypes.Break) Start.Decrement();
                else break;
            }
            while (true);
            Local = ToLocal(Pos);
            do
            {
                Local.Increment(Extract.length);
                if (Local.FileIndex == -1) break;
                Letter = GetLocal(Local);
                if (Letter.MarkingColorIndex == ColorIndex || Letter.Type == LetterTypes.Break) End.Increment(allContent.Count); 
                else break;
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
        internal PosDef Intersect(System.Windows.Point relPoint)
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
        public int Position(Vector PageSize)
        {
            this.Info.PageSize = PageSize;
            this.Info.State = PositionState.Normal;
            int FitCount = 0;
            Info.AllWhitespace = true;

            for (int i = 0; i < Content.Count(); i++)
            {
                var letter = Content.ElementAt(i);
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
