using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EPUBRenderer
{
    public enum LoadArg
    {
        none, setup
    }
    public class RenderBook
    {
        public readonly DateTime DateAdded;
        readonly private Epub epub;
        private PageFile[] pageFiles;
        public PosDef CurrPos;
        public string Title => epub == null ? "" : this.epub.Settings.Title;
        internal PageFile GetPage(int i, LoadArg type = LoadArg.setup)
        {
            var page = this.pageFiles[i];
            if (type == LoadArg.setup) page.Setup();
            return page;
        }
        internal async Task<PageFile> GetPositionedPage(int i)
        {
            var p = this.GetPage(i);
            await p.CalculatePages(pageSize, i);
            return p;
        }
        internal int pageFileCount => this.pageFiles.Length;
        public RenderBook(Epub epub, DateTime DateAdded, List<MrkDef> markings)
        {
            this.epub = epub;
            this.DateAdded = DateAdded;
            this.pageFiles = new PageFile[this.epub.PageCount];


            List<MrkDef>[] pageMarkings;
            pageMarkings = new List<MrkDef>[this.pageFileCount];

            for (int i = 0; i < pageMarkings.Length; i++) pageMarkings[i] = new();
            foreach (var Marking in markings)
            {
                var Pos = Marking.Pos;
                if (!this.PossiblyValid(Pos)) continue;
                pageMarkings[Pos.FileIndex].Add(Marking);//will be assigned on creation
            }

            for (int i = 0; i < this.pageFiles.Length; i++)
            {
                this.pageFiles[i] = new PageFile(this.epub.GetPage(i), this.epub.CSSExtract, pageMarkings[i]);
            }
        }
        private Vector pageSize;
        internal void PositionPrepare(Vector pageSize)
        {
            foreach (var page in this.pageFiles) page.PositioningTask = null;
            this.pageSize = pageSize;
        }
        public delegate void PageCountUpdatedEventHandler();
        public event PageCountUpdatedEventHandler PageCountUpdated;

        private int waitingCount = 0;
        private SemaphoreSlim sem = new(1, 1);
        internal async Task Position(PosDef openPage)
        {

            lock(sem)
            {
                this.waitingCount++;
            }
            await this.sem.WaitAsync();
            lock (sem)
            {
                this.waitingCount--;
            }
            //before and after
            int b = openPage.FileIndex;
            int a = openPage.FileIndex + 1;
            //calculate all pages starting from open
            for (int i = 0; i < pageFiles.Length; i++)
            {
                lock (sem)
                {
                    if (this.waitingCount > 0)
                    {
                        this.sem.Release();
                        return;
                    }
                }
                Task bTask = null;
                if (a < pageFiles.Length)
                {
                    bTask = this.pageFiles[a].CalculatePages(pageSize, a);
                }
                if (b >= 0)
                {
                    await this.pageFiles[b].CalculatePages(pageSize, b);
                }
                if (bTask is not null) await bTask;
                b--;
                a++;

                this.PageCountUpdated.Invoke();
            }
            this.sem.Release();


            //   for (int i = 0; i < PageFiles.Length; i++) pageOp(i); //better for debugging positioning

            // Parallel.For(0, this.PageFiles.Length, a => pageOp(a));

        }

        internal void RemoveMarking(PosDef start, PosDef end) => this.Iterate(start, end, (a, b) => a.MarkingColorIndex = 0);

        internal void AddMarking(PosDef start, PosDef end, byte colInd)
        {
            StringBuilder sb = new();
            this.Iterate(start, end, (a, b) =>
            {
                a.MarkingColorIndex = colInd;
                if (a is TextLetter textLetter && !textLetter.IsRuby) sb.Append(textLetter.OrigChar);
            });
            if (sb.Length > 0)
            {
                //to fix https://github.com/Georg2002/SimpleEPUBReader/issues/3
                try
                {
                    Clipboard.SetDataObject(sb.ToString(), true);
                }
                catch
                {
                    //nothing I guess?
                }
            }
        }

        private bool PossiblyValid(PosDef Pos) => Pos.FileIndex >= 0 && Pos.Letter >= 0 &&
                Pos.FileIndex < this.pageFileCount;


        internal Letter GetLetter(PosDef Pos)
        {
            if (Pos == PosDef.InvalidPosition) return null;
            if (Pos.FileIndex < this.pageFileCount && Pos.FileIndex >= 0)
            {
                var letters = this.GetPage(Pos.FileIndex).Content;
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
                Last = F == B.FileIndex;
                var letters = this.GetPage(F).Content;

                int startLetter = First ? A.Letter : 0;
                int endLetter = Last ? B.Letter + 1 : letters.Count;
                for (int Le = startLetter; Le < endLetter; Le++) Action(letters[Le], new PosDef(F, Le));
                First = false;
            }
        }

        internal Tuple<PosDef, PosDef> GetConnectedMarkings(PosDef Pos, RenderPage ShownPage) => ShownPage.GetConnectedMarkings(Pos, this.GetPage(this.CurrPos.FileIndex).Content);

        internal int GetPageCount() => this.pageFiles.Sum(a => a?.Pages?.Count ?? 0);

        internal int GetCurrentPage()
        {
            int Count = 0;
            for (int i = 0; i < this.pageFileCount; i++)
            {
                var file = this.GetPage(i, type: LoadArg.none);
                if (file.Pages.Count == 0) continue;
                if (file.Pages.Last().EndPos < CurrPos) Count += file.Pages.Count;
                else
                {
                    foreach (var Page in file.Pages)
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
            if (this.epub.toc == null) return Res;
            foreach (var Chapter in this.epub.toc.Chapters) Res.Add(Chapter.Title);
            return Res;
        }

        internal LibraryBook GetLibraryBook() => new()
        {
            CurrPos = CurrPos,
            FilePath = this.epub.FilePath,
            Title = this.epub.Settings.Title,
            Markings = this.GetMarkings(),
            DateAdded = DateAdded
        };
        private List<MrkDef> GetMarkings()
        {
            var Markings = new List<MrkDef>();
            this.Iterate(new PosDef(0, 0), this.GetLastPos(), (a, b) =>
              {
                  if (a.MarkingColorIndex != 0) Markings.Add(new MrkDef(b, a.MarkingColorIndex));
              });
            return Markings;
        }

        internal string GetSelection(PosDef selectionStart, PosDef selectionEnd)
        {
            string Text = "";
            if (selectionStart == PosDef.InvalidPosition || selectionEnd == PosDef.InvalidPosition) return Text;
            if (selectionEnd < selectionStart) (selectionStart, selectionEnd) = (selectionEnd, selectionStart);
            this.Iterate(selectionStart, selectionEnd, (a, b) =>
            {
                if (a.Type == LetterTypes.Letter)
                {
                    var TL = (TextLetter)a;
                    if (!TL.IsRuby) Text += TL.OrigChar;
                }
            });
            return Text.Trim(CharInfo.TrimCharacters);
        }

        internal void AddSelection(PosDef start, PosDef end) => this.Iterate(start, end, (a, b) => a.DictSelected = true);

        internal void RemoveSelection(PosDef start, PosDef end) => this.Iterate(start, end, (a, b) => a.DictSelected = false);

        internal PosDef GetLastPos()
        {
            var lastFile = this.GetPage(this.pageFileCount - 1);
            return new PosDef(this.pageFileCount - 1, lastFile.Content.Count - 1);
        }

        internal PosDef GetChapterPos(int chapterIndex)
        {
            var Chapter = this.epub.toc.Chapters[chapterIndex];
            var Index = -1;
            for (int i = 0; i < this.epub.PageCount; i++)
            {
                var page = this.epub.GetPage(i);
                if (page.FullName == Chapter.Source)
                {
                    Index = i;
                    break;
                }
            }
            var Pos = new PosDef(Index, 0);
            if (string.IsNullOrEmpty(Chapter.Jumppoint)) return Pos;
            var Page = this.GetPage(Index);
            Pos.Letter = Page.Content.FindIndex(a => a.Type == LetterTypes.Marker && ((MarkerLetter)a).Id == Chapter.Jumppoint);
            if (Pos.Letter == -1) Pos.Letter = 0;
            return Pos;
        }
    }
}
