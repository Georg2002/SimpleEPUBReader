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
        public string Title
        {
            get
            {
                return epub == null ? "" : epub.Settings.Title;
            }
        }


        public RenderBook(Epub epub)
        {
            this.epub = epub;
            PageFiles = new List<PageFile>();
            foreach (var Page in epub.Pages)
            {
                PageFiles.Add(new PageFile(Page, epub.CSSExtract));
            }
        }

        internal void Position(Vector pageSize)
        {
#if (DEBUG)
                 for (int i = 0; i < PageFiles.Count; i++)
                 {                
                     PageFiles[i].PositionText(pageSize, i);
                 }
#else
            Parallel.For(0, PageFiles.Count, a => PageFiles[a].PositionText(pageSize, a));
#endif
        }

        internal void RemoveMarking(PosDef firstHit, PosDef secondHit)
        {
            Iterate(firstHit, secondHit, a => a.MarkingColorIndex = 0);
        }

        internal void AddMarking(PosDef firstHit, PosDef secondHit, byte ColorIndex)
        {
            Iterate(firstHit, secondHit, a => a.MarkingColorIndex = ColorIndex);
        }

        private bool Valid(PosDef Pos)
        {
            return Pos.FileIndex >= 0 && Pos.Line >= 0 && Pos.Word >= 0 && Pos.Letter >= 0 &&
                Pos.FileIndex < PageFiles.Count && Pos.Line < PageFiles[Pos.FileIndex].Lines.Count &&
                Pos.Word < PageFiles[Pos.FileIndex].Lines[Pos.Line].Words.Count &&
                Pos.Letter < PageFiles[Pos.FileIndex].Lines[Pos.Line].Words[Pos.Word].Letters.Count;
        }

        internal void SetMarkings(List<MrkDef> markings)
        {
            foreach (var Marking in markings)
            {
                var Pos = Marking.Pos;
                if (Valid(Pos))
                {
                    PageFiles[Pos.FileIndex].Lines[Pos.Line].Words[Pos.Word].Letters[Pos.Letter].MarkingColorIndex = Marking.ColorIndex;
                }
            }
        }

        internal Letter GetLetter(PosDef Pos)
        {
            if (Pos == PosDef.InvalidPosition) return null;
            if (Pos.FileIndex < PageFiles.Count && Pos.FileIndex >= 0)
            {
                var Page = PageFiles[Pos.FileIndex];
                if (Pos.Line < Page.Lines.Count && Pos.Line >= 0)
                {
                    var Line = Page.Lines[Pos.Line];
                    if (Pos.Word < Line.Words.Count && Pos.Word >= 0)
                    {
                        var Word = Line.Words[Pos.Word];
                        if (Pos.Letter < Word.Letters.Count && Pos.Letter >= 0)
                        {
                            return Word.Letters[Pos.Letter];
                        }
                    }
                }
            }
            return null;
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
                Count += File.Pages.Count;
            }
            return Count;
        }

        internal int GetCurrentPage()
        {
            int Count = 0;
            foreach (var File in PageFiles)
            {
                if (File.Pages.Last().EndPos < CurrPos)
                {
                    Count += File.Pages.Count;
                }
                else
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
            }
            return Count;
        }

        internal List<string> GetChapters()
        {
            var Res = new List<string>();
            foreach (var Chapter in epub.toc.Chapters)
            {
                Res.Add(Chapter.Title);
            }
            return Res;
        }

        internal LibraryBook GetLibraryBook()
        {
            var Book = new LibraryBook
            {
                CurrPos = CurrPos,
                FilePath = epub.FilePath,
                Title = epub.Settings.Title,
                Markings = GetMarkings()
            };
            return Book;
        }

        private List<MrkDef> GetMarkings()
        {
            var Markings = new List<MrkDef>();
            for (int F = 0; F < PageFiles.Count; F++)
            {
                var File = PageFiles[F];
                for (int Li = 0; Li < File.Lines.Count; Li++)
                {
                    var Line = File.Lines[Li];
                    for (int W = 0; W < Line.Words.Count; W++)
                    {
                        var Word = Line.Words[W];
                        for (int Le = 0; Le < Word.Letters.Count; Le++)
                        {
                            var Letter = Word.Letters[Le];
                            if (Letter.MarkingColorIndex != 0)
                            {
                                Markings.Add(new MrkDef(new PosDef(F, Li, W, Le), Letter.MarkingColorIndex));
                            }
                        }
                    }
                }
            }
            return Markings;
        }

        internal string GetSelection(PosDef selectionStart, PosDef selectionEnd)
        {
            string Text = "";
            if (selectionStart == PosDef.InvalidPosition || selectionEnd == PosDef.InvalidPosition)
            {
                return Text;
            }
            if (selectionEnd < selectionStart)
            {
                var x = selectionEnd;
                selectionEnd = selectionStart;
                selectionStart = x;
            }
            Iterate(selectionStart, selectionEnd, a =>
            {
                if (a.Type == LetterTypes.Letter)
                {
                    var TL = (TextLetter)a;
                    if (!TL.IsRuby)
                    {
                        Text += TL.Character;
                    }
                }
            });
            return Text;
        }

        internal void AddSelection(PosDef selectionStart, PosDef selectionEnd)
        {
            Iterate(selectionStart, selectionEnd, a => a.DictSelected = true);
        }

        internal void RemoveSelection(PosDef selectionStart, PosDef selectionEnd)
        {
            Iterate(selectionStart, selectionEnd, a => a.DictSelected = false);
        }

        internal PosDef GetChapterPos(int chapterIndex)
        {
            var Chapter = epub.toc.Chapters[chapterIndex];
            var Index = epub.Pages.FindIndex(a => a.FullName == Chapter.Source);
            var Pos = new PosDef(Index, 0, 0, 0);
            if (string.IsNullOrEmpty(Chapter.Jumppoint)) return Pos;
            var Page = PageFiles[Index];
            for (int Li = 0; Li < Page.Lines.Count; Li++)
            {
                var Line = Page.Lines[Li];
                for (int W = 0; W < Line.Words.Count; W++)
                {
                    var Word = Line.Words[W];
                    if (Word.Letters[0].Type == LetterTypes.Marker)
                    {
                        var Marker = (MarkerLetter)Word.Letters[0];
                        if (Marker.Id == Chapter.Jumppoint)
                        {
                            Pos.Line = Li;
                            Pos.Word = W;
                            Pos.Letter = 0;
                        }
                    }
                }
            }
            return Pos;
        }
    }
}
