using EPUBParser;
using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
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
            void pageOp(int a)
            {
                PageFiles[a].CalculatePages(pageSize, a);
            }
#if (DEBUG)
            for (int i = 0; i < PageFiles.Count; i++) pageOp(i);
#else
            Parallel.For(0, PageFiles.Count, a => pageOp(a));
#endif
        }

        internal void RemoveMarking(PosDef start, PosDef end) => Iterate(start, end, (a, b) => a.MarkingColorIndex = 0);

        internal void AddMarking(PosDef start, PosDef end, byte colInd) => Iterate(start, end, (a, b) => a.MarkingColorIndex = colInd);

        private bool Valid(PosDef Pos) => Pos.FileIndex >= 0 && Pos.Letter >= 0 &&
                Pos.FileIndex < PageFiles.Count &&
                Pos.Letter < PageFiles[Pos.FileIndex].Content.Count;

        internal void SetMarkings(List<MrkDef> markings)
        {
            foreach (var Marking in markings)
            {
                var Pos = Marking.Pos;
                if (Valid(Pos)) PageFiles[Pos.FileIndex].Content[Pos.Letter].MarkingColorIndex = Marking.ColorIndex;
            }
        }

        internal Letter GetLetter(PosDef Pos)
        {
            if (Pos == PosDef.InvalidPosition) return null;
            if (Pos.FileIndex < PageFiles.Count && Pos.FileIndex >= 0)
            {
                var letters = PageFiles[Pos.FileIndex].Content;
                if (Pos.Letter < letters.Count && Pos.Letter >= 0) return letters[Pos.Letter];
            }
            return null;
        }

        private void Iterate(PosDef A, PosDef B, Action<Letter, PosDef> Action)
        {
            if (A.FileIndex == -1 || B.FileIndex == -1) return;

            if (A > B) (A, B) = (B, A);
            bool First = true;
            bool Last = false;
            for (int F = A.FileIndex; F < B.FileIndex + 1; F++)
            {
                var File = PageFiles[F];
                Last = F == B.FileIndex;
                var letters = File.Content;

                int startLetter = First ? A.Letter : 0;
                int endLetter = Last ? B.Letter + 1 : letters.Count;
                for (int Le = startLetter; Le < endLetter; Le++) Action(letters[Le], new PosDef(F, Le));
                First = false;
            }
        }

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, RenderPage ShownPage) => ShownPage.GetConnectedMarkings(Pos, PageFiles[CurrPos.FileIndex].Content);

        internal int GetPageCount() => this.PageFiles.Sum(a => a.Pages.Count);

        internal int GetCurrentPage()
        {
            int Count = 0;
            foreach (var File in PageFiles)
            {
                if (File.Pages.Last().EndPos < CurrPos) Count += File.Pages.Count;
                else
                {
                    foreach (var Page in File.Pages)
                    {
                        if (Page.StartPos > CurrPos) return Count;
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
            foreach (var Chapter in epub.toc.Chapters) Res.Add(Chapter.Title);
            return Res;
        }

        internal LibraryBook GetLibraryBook() => new LibraryBook
        {
            CurrPos = CurrPos,
            FilePath = epub.FilePath,
            Title = epub.Settings.Title,
            Markings = GetMarkings(),
            DateAdded = DateAdded
        };
        private List<MrkDef> GetMarkings()
        {
            var Markings = new List<MrkDef>();
            Iterate(new PosDef(0, 0), GetLastPos(), (a, b) =>
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
                    if (!TL.IsRuby) Text += TL.OrigChar;
                }
            });
            return Text.Trim(CharInfo.TrimCharacters);
        }

        internal void AddSelection(PosDef start, PosDef end) => Iterate(start, end, (a, b) => a.DictSelected = true);

        internal void RemoveSelection(PosDef start, PosDef end) => Iterate(start, end, (a, b) => a.DictSelected = false);

        internal PosDef GetLastPos()
        {
            var lastFile = PageFiles[PageFiles.Count - 1];
            return new PosDef(PageFiles.Count - 1, lastFile.Content.Count - 1);
        }

        internal PosDef GetChapterPos(int chapterIndex)
        {
            var Chapter = epub.toc.Chapters[chapterIndex];
            var Index = epub.Pages.FindIndex(a => a.FullName == Chapter.Source);
            var Pos = new PosDef(Index, 0);
            if (string.IsNullOrEmpty(Chapter.Jumppoint)) return Pos;
            var Page = PageFiles[Index];
            Pos.Letter = Page.Content.FindIndex(a => a.Type == LetterTypes.Marker && ((MarkerLetter)a).Id == Chapter.Jumppoint);
            if (Pos.Letter == -1) Pos.Letter = 0;
            return Pos;
        }
    }
}
