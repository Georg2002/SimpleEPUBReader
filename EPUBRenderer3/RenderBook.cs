﻿using EPUBParser;
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
        readonly private Epub epub;
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
            for (int i = 0; i < PageFiles.Count; i++)
            {
                if (i == 10)
                {
                    ;
                }
                PageFiles[i].PositionText(pageSize, i);
            }
            // Parallel.For(0, PageFiles.Count, a => PageFiles[a].PositionText(pageSize, a));
        }

        internal void RemoveMarking(PosDef firstHit, PosDef secondHit)
        {
            Iterate(firstHit, secondHit, a => a.MarkingColorIndex = 0);
        }

        internal void AddMarking(PosDef firstHit, PosDef secondHit, byte ColorIndex)
        {
            Iterate(firstHit, secondHit, a => a.MarkingColorIndex = ColorIndex);
        }

        private bool valid(PosDef Pos)
        {
            return Pos.FileIndex < PageFiles.Count && Pos.Line < PageFiles[Pos.FileIndex].Lines.Count &&
                Pos.Word < PageFiles[Pos.FileIndex].Lines[Pos.Line].Words.Count &&
                Pos.Letter < PageFiles[Pos.FileIndex].Lines[Pos.Line].Words[Pos.Word].Letters.Count
                && Pos.FileIndex >= 0 && Pos.Line >= 0 && Pos.Word >= 0 && Pos.Letter >= 0;
        }

        private void Iterate(PosDef A, PosDef B, Action<Letter> Action)
        {
            if (A.FileIndex == -1 || B.FileIndex == -1) return;


            if (A > B)
            {
                var Temp = A;
                A = B;
                B = Temp;
            }
            bool First = true;
            bool Last = false;
            for (int F = A.FileIndex; F < B.FileIndex + 1; F++)
            {
                var File = PageFiles[F];
                Last = F == B.FileIndex;
                int StartLi = F == A.FileIndex && First ? A.Line : 0;
                int EndLi = Last ? B.Line + 1 : File.Lines.Count;

                for (int Li = StartLi; Li < EndLi; Li++)
                {
                    Last = F == B.FileIndex && Li == B.Line;
                    var Line = File.Lines[Li];
                    int StartW = Li == A.Line && First ? A.Word : 0;
                    int EndW = Last ? B.Word + 1 : Line.Words.Count;


                    for (int W = StartW; W < EndW; W++)
                    {
                        Last = F == B.FileIndex && Li == B.Line && W == B.Word;
                        var Word = Line.Words[W];
                        int StartLe = W == A.Word && First ? A.Letter : 0;
                        int EndLe = Last ? B.Letter + 1 : Word.Letters.Count;
                        for (int Le = StartLe; Le < EndLe; Le++)
                        {
                            var Letter = Word.Letters[Le];
                            Action(Letter);
                        }
                        First = false;
                    }
                }
            }
        }

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, RenderPage ShownPage)
        {
            return ShownPage.GetConnectedMarkings(Pos, PageFiles[CurrPos.FileIndex].Lines);
        }

        internal int GetPageCount()
        {
            int Count = 0;
            foreach (var File in PageFiles)
            {
                foreach (var Page in File.Pages)
                {
                    Count++;
                }
            }
            return Count;
        }

        internal int GetCurrentPage()
        {
            int Count = 0;
            foreach (var File in PageFiles)
            {
                foreach (var Page in File.Pages)
                {
                    if (Page.StartPos > CurrPos)
                    {
                        return Count;
                    }
                    Count++;
                }
            }
            return Count;
        }
    }
}