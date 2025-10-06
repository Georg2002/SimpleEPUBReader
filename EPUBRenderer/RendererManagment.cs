using EPUBParser;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

        public bool Rendering { get; private set; } = false;

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
            var length = CurrBook.GetPageFile(SelectionStart.FileIndex).Content.Count;
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
            if (this.CurrBook is not null) this.CurrBook.PageCountUpdated -= this.UpdatePageCount;
            this.CurrBook = new RenderBook(epub, DateAdded, Markings);
            this.CurrBook.PageCountUpdated += this.UpdatePageCount;

            async Task loadAsync()
            {
                await this.positioningSemaphore.WaitAsync();
                CurrBook.PositionPrepare(this.PageSize);
                await this.OpenPage(Position);
                this.positioningSemaphore.Release();
                await CurrBook.Position(Position);
                //  await posTask;
            }
            loadAsync().CatchAll();
        }

        private void UpdatePageCount() => this.Dispatcher.Invoke(()=>this.Refresh());

        public async Task OpenPage(PosDef Position)
        {
            if (CurrBook == null) return;
            CurrBook.CurrPos = Position;
            var PageFile = await CurrBook.GetPositionedPage(Position.FileIndex);
            this.ShownPage = PageFile.GetShownPage(Position);
            this.Refresh(rerender:true);
        }

        public async Task Switch(int dir)
        {
            if (CurrBook == null) return;

            int fileIndex = CurrBook.CurrPos.FileIndex;
            int pageIndex = (await CurrBook.GetPositionedPage(fileIndex)).GetPageIndex(ShownPage);
            pageIndex += dir;
            while (pageIndex < 0 || pageIndex >= (await CurrBook.GetPositionedPage(fileIndex)).GetPageCount())
            {
                if (pageIndex < 0)
                {
                    fileIndex--;
                    if (fileIndex < 0)
                    {
                        fileIndex = 0;
                        pageIndex = 0;
                        break;
                    }
                    pageIndex += (await CurrBook.GetPositionedPage(fileIndex)).GetPageCount();
                }
                else
                {
                    fileIndex++;
                    if (fileIndex >= CurrBook.pageFileCount)
                    {
                        fileIndex = CurrBook.pageFileCount - 1;
                        pageIndex = (await CurrBook.GetPositionedPage(fileIndex)).GetPageCount() - 1;
                        break;
                    }
                    pageIndex -= (await CurrBook.GetPositionedPage(fileIndex - 1)).GetPageCount();
                }
            }

            //will succeed opening some page, even if dist is not correct
            this.OpenPage(CurrBook.GetPageFile(fileIndex).GetPage(pageIndex, safe: true).StartPos).CatchAll();
        }

        private void SetCurrPos(PosDef pos)
        {
            if (CurrBook != null && !pos.IsInvalid) CurrBook.CurrPos = pos;
        }
        public bool StartMarking(Point relPoint)
        {
            bool Valid = false;
            if (CurrBook is null || this.ShownPage is null) return Valid;

            this.FirstHit = ShownPage.Intersect(relPoint, useFuzzyHit: true);
            this.SetCurrPos(FirstHit);
            Valid = !FirstHit.IsInvalid;

            return Valid;
        }

        private DateTime lastTempMarkDraw = DateTime.MinValue;
        public void DrawTempMarking(Point relPoint, byte ColorIndex, bool ignoreInterval = false)
        {
            var newSecondHit = ShownPage.Intersect(relPoint, useFuzzyHit: true);
            if (newSecondHit == this.SecondHit) return;
            if (!ignoreInterval && DateTime.Now.Subtract(lastTempMarkDraw).TotalMilliseconds < 1 / 60.0) return;
            lastTempMarkDraw = DateTime.Now;

            CurrBook.RemoveMarking(FirstHit, SecondHit);
            this.SecondHit = newSecondHit;
            this.SetCurrPos(SecondHit);
            this.CurrBook.AddMarking(FirstHit, SecondHit, ColorIndex);
            this.Refresh();
        }

        public void FinishMarking(Point relPoint, byte ColorIndex)
        {
            this.DrawTempMarking(relPoint, ColorIndex, ignoreInterval: true);
            //to fix https://github.com/Georg2002/SimpleEPUBReader/issues/3
            try
            {
                Clipboard.SetDataObject(this.CurrBook.MarkedText, true);
            }
            catch
            {
                //nothing I guess?
            }
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
            Application.Current.Dispatcher.Invoke(() => this.Refresh());
            this.RemoveSelection();
            this.SelectionEnd = ShownPage.Intersect(relPoint);
            this.SetCurrPos(SelectionEnd);
            if (!SelectionStart.IsInvalid && !SelectionEnd.IsInvalid) CurrBook.AddSelection(SelectionStart, SelectionEnd);
        }

        public string GetSelection() => CurrBook.GetSelection(SelectionStart, SelectionEnd);

        public int GetPageCount() => CurrBook?.GetPageCount() ?? 0;

        public int GetCurrentPage() => CurrBook?.GetCurrentPage() ?? 0;

        public List<string> GetChapters() => CurrBook?.GetChapters() ?? new List<string>();

        public async Task SetChapter(int chapterIndex)
        {
            PosDef Pos = CurrBook.GetChapterPos(chapterIndex);
            await this.OpenPage(Pos);
        }
        public LibraryBook GetCurrentBook() => CurrBook?.GetLibraryBook() ?? new LibraryBook() { CurrPos = PosDef.InvalidPosition };

        public void DeactivateSelection()
        {
            this.RemoveSelection();
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
        }

        private void Refresh(bool rerender = false)
        {
            if (rerender) this.Rendering = true;
            this.InvalidateVisual();
        }
        private SemaphoreSlim positioningSemaphore = new(1, 1);
        private bool recalculatingSizeWaiting = false;
        private object recalculatingLockO = new();
        private void Renderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (this.recalculatingLockO)
            {
                this.PageSize = new Vector(this.ActualWidth, this.ActualHeight);
                if (CurrBook is null || this.recalculatingSizeWaiting) return;
            }

            async Task loadAsync()
            {
                lock (this.recalculatingLockO) recalculatingSizeWaiting = true;
                await this.positioningSemaphore.WaitAsync();
                lock (this.recalculatingLockO) recalculatingSizeWaiting = false;

                this.PageSize = new Vector(this.ActualWidth, this.ActualHeight);
                CurrBook.PositionPrepare(this.PageSize);
                await this.OpenPage(CurrBook.CurrPos);
                this.positioningSemaphore.Release();
                await CurrBook.Position(CurrBook.CurrPos);
            }
            this.Rendering = true;

            loadAsync().CatchAll();
        }
    }
}
