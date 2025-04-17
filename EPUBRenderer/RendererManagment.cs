using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EPUBParser;

namespace EPUBRenderer
{
    public partial class Renderer : FrameworkElement
    {
        public RenderBook CurrBook;
        Vector PageSize;
        RenderPage ShownPage = null;
        PosDef FirstHit = PosDef.InvalidPosition;
        PosDef SecondHit = PosDef.InvalidPosition;
        public Brush[] MarkingColors;
        private PosDef SelectionEnd = PosDef.InvalidPosition;
        private PosDef SelectionStart = PosDef.InvalidPosition;
        private bool Rerender = false;


        public Renderer()
        {
            SizeChanged += this.Renderer_SizeChanged;
            this.MinHeight = 100;
            this.MinWidth = 100;
        }

        public void MoveSelection(int front, int end)
        {
            if (SelectionEnd == PosDef.InvalidPosition || SelectionStart == PosDef.InvalidPosition)
            {
                return;
            }
            this.RemoveSelection();
            if (SelectionStart > SelectionEnd)
            {
                (SelectionEnd, SelectionStart) = (SelectionStart, SelectionEnd);
            }
            var EndOld = SelectionEnd;
            var StartOld = SelectionStart;
            var length = CurrBook.PageFiles[SelectionStart.FileIndex].Content.Count;
            this.MoveSelectionPoints(front, end, length);

            Letter StartLetter = CurrBook.GetLetter(SelectionStart);
            Letter EndLetter = CurrBook.GetLetter(SelectionEnd);
            if (StartLetter == null || EndLetter == null)
            {
                this.SelectionEnd = EndOld;
                this.SelectionStart = StartOld;
            }
            //revert if overtook
            if (SelectionStart > SelectionEnd)
            {
                this.SelectionStart = StartOld;
                this.SelectionEnd = EndOld;
            }
            CurrBook.AddSelection(SelectionStart, SelectionEnd);
            this.Refresh();
        }

        private void MoveSelectionPoints(int front, int end, int letterCount)
        {
            var EndOld = SelectionEnd;
            var StartOld = SelectionStart;
            if (front > 0) SelectionStart.Increment(letterCount);
            else if (front < 0) SelectionStart.Decrement();
            if (SelectionStart.FileIndex == -1)
            {
                this.SelectionStart = StartOld;
                return;
            }
            if (end > 0) SelectionEnd.Increment(letterCount);
            else if (end < 0) SelectionEnd.Decrement();
            if (SelectionEnd.FileIndex == -1)
            {
                this.SelectionEnd = EndOld;
                return;
            }

            Letter StartLetter = CurrBook.GetLetter(SelectionStart);
            Letter EndLetter = CurrBook.GetLetter(SelectionEnd);

            if (StartLetter == null || EndLetter == null) return;
            if (StartLetter.Type == LetterTypes.Letter && EndLetter.Type == LetterTypes.Letter)
            {
                var StartTL = (TextLetter)StartLetter;
                var EndTL = (TextLetter)EndLetter;
                if (!StartTL.IsRuby && !EndTL.IsRuby) return;
            }
            this.MoveSelectionPoints(front, end, letterCount);
        }
        public void LoadBook(string Path, DateTime DateAdded, PosDef Position = new PosDef(), List<MrkDef> Markings = null)
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path) || !Path.ToLower().EndsWith(".epub"))
            {
                MessageBox.Show($"Path {Path} invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
            Markings ??= new List<MrkDef>();
            Epub epub = new Epub(Path);
            this.CurrBook = new RenderBook(epub, DateAdded);
            this.SetMarkings(Markings);
            CurrBook.Position(PageSize);
            this.OpenPage(Position);
        }

        private void SetMarkings(List<MrkDef> Markings)
        {
            CurrBook.SetMarkings(Markings);
        }
        private void LoadAdjacentPages()
        {

            void load(int file, int page)
            {
                foreach (var textLetter in CurrBook.PageFiles[file].Pages[page].Content.Where(a => a is TextLetter).Cast<TextLetter>())
                {
                    (var typeface, var index) = textLetter.GetRenderingInfo();
                    GetAdvanceWidth(index, typeface);
                }
            }

            (int nextFile, int nextPage) = this.GetFileAndPageForSwitch(1);
            (int prevFile, int prevPage) = this.GetFileAndPageForSwitch(-1);

            Task.Run(() =>
             {
                 Thread.Sleep(2000);
                 load(nextFile, nextPage);
                 Thread.Sleep(2000);
                 load(prevFile, prevPage);
             });
        }

        public void OpenPage(PosDef Position)
        {
            if (CurrBook == null) return;
            CurrBook.CurrPos = Position;
            var PageFile = CurrBook.PageFiles[Position.FileIndex];
            this.ShownPage = PageFile.Pages.Find(a => a.Within(Position));
            this.Refresh();
            this.LoadAdjacentPages();
        }
        private Tuple<int, int> GetFileAndPageForSwitch(int dir)
        {
            int FileIndex = CurrBook.CurrPos.FileIndex;
            int PageIndex = CurrBook.PageFiles[FileIndex].Pages.IndexOf(ShownPage);
            PageIndex += dir;
            while (PageIndex < 0 || PageIndex >= CurrBook.PageFiles[FileIndex].Pages.Count)
            {
                if (PageIndex < 0)
                {
                    FileIndex--;
                    if (FileIndex < 0)
                    {
                        FileIndex = 0;
                        PageIndex = 0;
                        break;
                    }
                    PageIndex += CurrBook.PageFiles[FileIndex].Pages.Count;
                }
                else
                {
                    FileIndex++;
                    if (FileIndex >= CurrBook.PageFiles.Length)
                    {
                        FileIndex = CurrBook.PageFiles.Length - 1;
                        PageIndex = CurrBook.PageFiles[FileIndex].Pages.Count - 1;
                        break;
                    }
                    PageIndex -= CurrBook.PageFiles[FileIndex - 1].Pages.Count;
                }
            }
            return new(FileIndex, PageIndex);
        }
        public void Switch(int Dir)
        {
            if (CurrBook == null) return;
            (var FileIndex, var PageIndex) = this.GetFileAndPageForSwitch(Dir);
            this.OpenPage(CurrBook.PageFiles[FileIndex].Pages[PageIndex].StartPos);
        }

        private void SetCurrPos(PosDef pos)
        {
            if (CurrBook != null && !pos.IsInvalid) CurrBook.CurrPos = pos;
        }

        public bool StartMarking(Point relPoint)
        {
            bool Valid = false;
            if (CurrBook != null)
            {
                this.FirstHit = ShownPage.Intersect(relPoint);
                this.SetCurrPos(FirstHit);
                Valid = !FirstHit.IsInvalid;
            }
            return Valid;
        }

        public void DrawTempMarking(Point relPoint, byte ColorIndex)
        {
            CurrBook.RemoveMarking(FirstHit, SecondHit);
            this.SecondHit = ShownPage.Intersect(relPoint);
            this.SetCurrPos(SecondHit);
            CurrBook.AddMarking(FirstHit, SecondHit, ColorIndex);
            this.Refresh();
        }

        public void FinishMarking(Point relPoint, byte ColorIndex)
        {
            this.DrawTempMarking(relPoint, ColorIndex);
            this.SecondHit = PosDef.InvalidPosition;
        }

        public void RemoveMarking(Point relPoint)
        {
            if (CurrBook != null)
            {
                PosDef Hit = ShownPage.Intersect(relPoint);
                this.SetCurrPos(Hit);
                if (Hit.IsInvalid) return;
                (PosDef A, PosDef B) = CurrBook.GetConnectedMarkings(Hit, ShownPage);
                CurrBook.RemoveMarking(A, B);
            }
            this.Refresh();
        }

        public bool StartSelection(Point relPoint)
        {
            bool Valid = false;
            if (CurrBook != null)
            {
                var NewStart = ShownPage.Intersect(relPoint);
                Valid = NewStart.FileIndex != -1;
                if (Valid) this.SelectionStart = NewStart;
            }
            return Valid;
        }

        public void RemoveSelection()
        {
            if (CurrBook == null) return;
            CurrBook.RemoveSelection(SelectionStart, SelectionEnd);
        }

        public void ContinueSelection(Point relPoint)
        {
            this.Refresh();
            this.RemoveSelection();
            this.SelectionEnd = ShownPage.Intersect(relPoint);
            this.SetCurrPos(SelectionEnd);
            if (!SelectionStart.IsInvalid && !SelectionEnd.IsInvalid) CurrBook.AddSelection(SelectionStart, SelectionEnd);
        }

        public string GetSelection() => CurrBook.GetSelection(SelectionStart, SelectionEnd);

        public int GetPageCount() => CurrBook?.GetPageCount() ?? 0;

        public int GetCurrentPage() => CurrBook?.GetCurrentPage() ?? 0;

        public List<string> GetChapters() => CurrBook?.GetChapters() ?? new List<string>();

        public void SetChapter(int chapterIndex)
        {
            PosDef Pos = CurrBook.GetChapterPos(chapterIndex);
            this.OpenPage(Pos);
        }
        public LibraryBook GetCurrentBook() => CurrBook?.GetLibraryBook() ?? new LibraryBook() { CurrPos = PosDef.InvalidPosition };

        public void DeactivateSelection()
        {
            this.RemoveSelection();
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
        }

        private void Refresh()
        {
            this.Rerender = true;
            this.InvalidateVisual();
        }
        private void Renderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.PageSize = new Vector(this.ActualWidth, this.ActualHeight);
            if (CurrBook != null)
            {
                CurrBook.Position(PageSize);
                this.OpenPage(CurrBook.CurrPos);
            }
            this.Rerender = true;
        }
    }
}
