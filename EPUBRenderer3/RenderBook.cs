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
        public DateTime DateAdded;
        readonly private Epub epub;
        internal List<PageFile> PageFiles;
        public PosDef CurrPos;
        public string Title => epub == null ? "" : epub.Settings.Title;

        public RenderBook(Epub epub, DateTime DateAdded)
        {
            this.epub = epub;
            this.DateAdded = DateAdded;
            PageFiles = new List<PageFile>();
            foreach (var Page in epub.Pages) PageFiles.Add(new PageFile(Page, epub.CSSExtract));
        }

        internal void Position(Vector pageSize)
        {
#if (DEBUG)
            for (int i = 0; i < PageFiles.Count; i++) PageFiles[i].PositionText(pageSize, i);
#else
            Parallel.For(0, PageFiles.Count, a => PageFiles[a].PositionText(pageSize, a));
#endif
        }

        internal void RemoveMarking(PosDef start, PosDef end) => Iterate(start, end, (a, b) => a.MarkingColorIndex = 0);

        internal void AddMarking(PosDef start, PosDef end, byte colInd) => Iterate(start, end, (a, b) => a.MarkingColorIndex = colInd);

        private bool Valid(PosDef Pos)
        {
            return Pos.FileIndex >= 0 && Pos.Word >= 0 && Pos.Letter >= 0 &&
                Pos.FileIndex < PageFiles.Count &&
                Pos.Word < PageFiles[Pos.FileIndex].Words.Count &&
                Pos.Letter < PageFiles[Pos.FileIndex].Words[Pos.Word].Letters.Count;
        }

        internal void SetMarkings(List<MrkDef> markings)
        {
            foreach (var Marking in markings)
            {
                var Pos = Marking.Pos;
                if (Valid(Pos)) PageFiles[Pos.FileIndex].Words[Pos.Word].Letters[Pos.Letter].MarkingColorIndex = Marking.ColorIndex;
            }
        }

        internal Letter GetLetter(PosDef Pos)
        {
            if (Pos == PosDef.InvalidPosition) return null;
            if (Pos.FileIndex < PageFiles.Count && Pos.FileIndex >= 0)
            {
                var words = PageFiles[Pos.FileIndex].Words;     
                if (Pos.Word < words.Count && Pos.Word >= 0)
                {
                    var Word = words[Pos.Word];
                    if (Pos.Letter < Word.Letters.Count && Pos.Letter >= 0) return Word.Letters[Pos.Letter];                  
                }
            }
            return null;
        }

        private void Iterate(PosDef A, PosDef B, Action<Letter, PosDef> Action)
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
                var Words = File.Words;

                int StartW = F == A.FileIndex && First ? A.Word : 0;
                int EndW = Last ? B.Word + 1 : Words.Count;

                for (int W = StartW; W < EndW; W++)
                {
                    Last = F == B.FileIndex && W == B.Word;
                    var Word = Words[W];
                    int StartLe = W == A.Word && First ? A.Letter : 0;
                    int EndLe = Last ? B.Letter + 1 : Word.Letters.Count;
                    for (int Le = StartLe; Le < EndLe; Le++)
                    {
                        var Letter = Word.Letters[Le];
                        Action(Letter, new PosDef(F, W, Le));
                    }
                    First = false;

                }
            }
        }

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, RenderPage ShownPage)
        {
            return ShownPage.GetConnectedMarkings(Pos, PageFiles[CurrPos.FileIndex].Words);
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
            if (epub.toc == null) return Res;
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
                Markings = GetMarkings(),
                DateAdded = DateAdded
            };
            return Book;
        }

        private List<MrkDef> GetMarkings()
        {
            var Markings = new List<MrkDef>();
            Iterate(new PosDef(0, 0, 0), GetLastPos(), (a, b) =>
              {
                  if (a.MarkingColorIndex != 0) Markings.Add(new MrkDef(b, a.MarkingColorIndex));
              });
            return Markings;
        }

        internal string GetSelection(PosDef selectionStart, PosDef selectionEnd)
        {
            string Text = "";
            if (selectionStart == PosDef.InvalidPosition || selectionEnd == PosDef.InvalidPosition) return Text;
            if (selectionEnd < selectionStart)
            {
                var x = selectionEnd;
                selectionEnd = selectionStart;
                selectionStart = x;
            }
            Iterate(selectionStart, selectionEnd, (a, b) =>
            {
                if (a.Type == LetterTypes.Letter)
                {
                    var TL = (TextLetter)a;
                    if (!TL.IsRuby) Text += TL.Character;
                }
            });
            return Text;
        }

        internal void AddSelection(PosDef start, PosDef end) => Iterate(start, end, (a, b) => a.DictSelected = true);

        internal void RemoveSelection(PosDef start, PosDef end) => Iterate(start, end, (a, b) => a.DictSelected = false);

        internal PosDef GetLastPos()
        {
            var lastFile = PageFiles[PageFiles.Count - 1];
            var lastWord = lastFile.Words.Last();
            return new PosDef(PageFiles.Count - 1, lastFile.Words.Count - 1, lastWord.Letters.Count - 1);
        }

        internal PosDef GetChapterPos(int chapterIndex)
        {
            var Chapter = epub.toc.Chapters[chapterIndex];
            var Index = epub.Pages.FindIndex(a => a.FullName == Chapter.Source);
            var Pos = new PosDef(Index, 0, 0);
            if (string.IsNullOrEmpty(Chapter.Jumppoint)) return Pos;
            var Page = PageFiles[Index];
            for (int W = 0; W < Page.Words.Count; W++)
            {
                var Word = Page.Words[W];
                if (Word.Letters[0].Type == LetterTypes.Marker)
                {
                    var Marker = (MarkerLetter)Word.Letters[0];
                    if (Marker.Id == Chapter.Jumppoint)
                    {
                        Pos.Word = W;
                        Pos.Letter = 0;
                    }
                }
            }
            return Pos;
        }
    }
}
