using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer3
{
    public class RenderBook
    {
        private Epub epub;
        internal List<PageFile> PageFiles;
        public PosDef CurrPos;

        public RenderBook(Epub epub)
        {
            this.epub = epub;
            PageFiles = new List<PageFile>();
            int i = 0;
            foreach (var Page in epub.Pages)
            {
                if (i == 10)
                {
                    ;
                }
                i++;
                PageFiles.Add(new PageFile(Page));
            }
        }

        internal void Position(Vector pageSize)
        {
            //  for (int i = 0; i < PageFiles.Count; i++)
            //  {
            //       if (i==10)
            //       {
            //           ;
            //       }
            //      PageFiles[i].PositionText(pageSize, i);
            //  }
            Parallel.For(0, PageFiles.Count, a => PageFiles[a].PositionText(pageSize, a));
        }

        internal void RemoveMarking(PosDef firstHit, PosDef secondHit)
        {
            Iterate(firstHit, secondHit, a => a.MarkingColorIndex = 0);
        }

        internal void AddMarking(PosDef firstHit, PosDef secondHit, byte ColorIndex)
        {
            Iterate(firstHit, secondHit, a => a.MarkingColorIndex = ColorIndex);
        }

        private void Iterate(PosDef A, PosDef B, Action<Letter> Action)
        {
            if (A < B)
            {
                var Temp = A;
                A = B;
                B = Temp;
            }
            for (int F = A.FileIndex; F < B.FileIndex; F++)
            {
                var File = PageFiles[F];
                int StartLi = F == A.FileIndex ? A.Line : 0;
                int EndLi = F == B.FileIndex - 1 ? B.Line + 1 : File.Lines.Count;

                for (int Li = StartLi; Li < EndLi; Li++)
                {
                    var Line = File.Lines[Li];
                    int StartW = Li == A.Line ? A.Word : 0;
                    int EndW = Li == B.Line - 1 ? B.Word + 1 : Line.Words.Count;

                    for (int W = StartW; W < EndW; W++)
                    {
                        var Word = Line.Words[W];
                        int StartLe = W == A.Word ? A.Letter : 0;
                        int EndLe = W == B.Word - 1 ? B.Letter + 1 : Word.Letters.Count;
                        for (int Le = StartLe; Le < EndLe; Le++)
                        {
                            var Letter = Word.Letters[Le];
                            Action(Letter);                           
                        }
                    }
                }
            }
        }
    }
}
