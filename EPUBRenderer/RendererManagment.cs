using EPUBParser;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EPUBRenderer
{
    public partial class Renderer : SKElementCust
    {
        public static float WindowScale { get; private set; } = 1.0f;//Scale from Windows display settings

        public RenderBook CurrBook;
        Vector PageSize;
        RenderPage ShownPage = null;
        PosDef FirstHit = PosDef.InvalidPosition;
        PosDef SecondHit = PosDef.InvalidPosition;
        public SKPaint[] MarkingColors;
        private PosDef SelectionEnd = PosDef.InvalidPosition;
        private PosDef SelectionStart = PosDef.InvalidPosition;
        private readonly SKPaint blackPaint = new() { Color = SKColors.Black, IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKSamplingOptions samplingOptions = new(SKFilterMode.Linear, SKMipmapMode.Linear);
        public bool Rendering { get; private set; } = false;

        public delegate void RefreshedEventHandler();
        public event RefreshedEventHandler RefreshedEvent;

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
            LOGPIXELSY = 90,
        }
        private static readonly object lockO = new();
        static Renderer()
        {
            using System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
            int logpixelsy = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
            g.ReleaseHdc(desktop);

            float screenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;
            float dpiScalingFactor = (float)logpixelsy / (float)96;

            if (screenScalingFactor > 1 || dpiScalingFactor > 1)
            {
                Renderer.WindowScale = dpiScalingFactor;
            }
        }

        public Renderer()
        {
            SizeChanged += this.Renderer_SizeChanged;
            this.MinHeight = 100;
            this.MinWidth = 100;

            this.WorkInputQueue();
        }
        private static SKFont GetFont(SKTypeface tf, float size) => new()
        {
            Size = size,
            Typeface = tf,
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias,
        };
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
            this.QueueRefresh();
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

            if (Position.FileIndex >= this.CurrBook.pageFileCount) Position.FileIndex = this.CurrBook.pageFileCount - 1;

            async Task loadAsync()
            {
                this.PageSize = this.GetPageSize();
                CurrBook.PositionPrepare(this.PageSize);
                await this.OpenPage(Position);
                CurrBook.Position(Position).CatchAll();
            }
            this.InputQueue.Add(loadAsync);
        }

        private void UpdatePageCount() => this.QueueRefresh();

        public async Task OpenPage(PosDef Position)
        {
            if (CurrBook == null) return;
            this.FirstHit = this.SecondHit = PosDef.InvalidPosition;

            CurrBook.CurrPos = Position;
            var PageFile = await CurrBook.GetPositionedPage(Position.FileIndex);
            this.ShownPage = PageFile.GetShownPage(Position);
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
            await this.OpenPage(CurrBook.GetPageFile(fileIndex).GetPage(pageIndex, safe: true).StartPos);
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
        public bool MarkingInProgress() => this.FirstHit != PosDef.InvalidPosition || this.SelectionStart != PosDef.InvalidPosition;

        private DateTime lastTempMarkDraw = DateTime.MinValue;
        public void DrawTempMarking(Point relPoint, byte ColorIndex, bool ignoreInterval = false)
        {
            var lessThanFrame = DateTime.Now.Subtract(lastTempMarkDraw).TotalMilliseconds < 1 / 60.0;
            async Task fun()
            {

                var newSecondHit = ShownPage.Intersect(relPoint, useFuzzyHit: true);
                if (newSecondHit == this.SecondHit) return;
                if (!ignoreInterval && lessThanFrame) return;
                lastTempMarkDraw = DateTime.Now;

                CurrBook.RemoveMarking(FirstHit, SecondHit);
                this.SecondHit = newSecondHit;
                this.SetCurrPos(SecondHit);
                this.CurrBook.AddMarking(FirstHit, SecondHit, ColorIndex);

                await Task.CompletedTask;
            }
            this.InputQueue.Add(fun);
        }

        public void FinishMarking(Point relPoint, byte ColorIndex)
        {
            this.DrawTempMarking(relPoint, ColorIndex, ignoreInterval: true);
            //to fix https://github.com/Georg2002/SimpleEPUBReader/issues/3
            if (!string.IsNullOrEmpty(this.CurrBook.MarkedText))
            {
                try
                {
                    Clipboard.SetDataObject(this.CurrBook.MarkedText, true);
                }
                catch
                {
                    //nothing I guess?
                }
            }
            this.FirstHit = this.SecondHit = PosDef.InvalidPosition;
        }

        public void RemoveMarking(Point relPoint)
        {
            async Task fun()
            {
                if (CurrBook != null)
                {
                    PosDef Hit = ShownPage.Intersect(relPoint);
                    this.SetCurrPos(Hit);
                    if (Hit.IsInvalid) return;
                    (PosDef A, PosDef B) = CurrBook.GetConnectedMarkings(Hit, ShownPage);
                    CurrBook.RemoveMarking(A, B);
                }
                await Task.CompletedTask;
            }
            this.InputQueue.Add(fun);
        }

        public bool StartSelection(Point relPoint)
        {
            bool Valid = false;
            if (this.CurrBook != null && this.ShownPage is not null)
            {
                this.RemoveSelection();
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
            this.RemoveSelection();
            this.SelectionEnd = ShownPage.Intersect(relPoint);
            this.SetCurrPos(SelectionEnd);
            if (!SelectionStart.IsInvalid && !SelectionEnd.IsInvalid) CurrBook.AddSelection(SelectionStart, SelectionEnd);
            this.QueueRefresh();
        }

        public string GetSelection() => CurrBook.GetSelection(SelectionStart, SelectionEnd);

        public int GetPageCount() => CurrBook?.GetPageCount() ?? 0;

        public int GetCurrentPage() => CurrBook?.GetCurrentPage() ?? 0;

        public List<string> GetChapters() => CurrBook?.GetChapters() ?? new List<string>();

        public void SetChapter(int chapterIndex)
        {
            PosDef Pos = CurrBook.GetChapterPos(chapterIndex);
            this.InputQueue.Add(() => this.OpenPage(Pos));
        }
        public LibraryBook GetCurrentBook() => CurrBook?.GetLibraryBook() ?? new LibraryBook() { CurrPos = PosDef.InvalidPosition };

        public void DeactivateSelection()
        {
            this.RemoveSelection();
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
            this.QueueRefresh();
        }

        private readonly SemaphoreSlim  renderingSemaphore = new(0, 1);
        private async Task Refresh()
        {
            lock (this.renderLockObject) this.Rendering = true;
            this.Dispatcher.Invoke(this.InvalidateVisual);
            await this.renderingSemaphore.WaitAsync();
            RefreshedEvent?.Invoke();
        }

        private void Renderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.CurrBook is null) return;

            async Task loadAsync()
            {
                this.PageSize = this.GetPageSize();
                CurrBook.PositionPrepare(this.PageSize);
                await this.OpenPage(CurrBook.CurrPos);
                CurrBook.Position(CurrBook.CurrPos).CatchAll();
            }

            this.InputQueue.Add(loadAsync);
        }



        public readonly BlockingCollection<Func<Task>> InputQueue = new(new ConcurrentQueue<Func<Task>>());

        public void QueueRefresh() => this.InputQueue.Add(null);
        private void WorkInputQueue()
        {
            //  this.TestLoop();
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        var func = InputQueue.Take();
                        if (func is not null)
                        {
                            await func?.Invoke();
                        }
                        await this.Refresh();
                    }
                }
                catch (TaskCanceledException)
                {
                    //expected behavior on shutdown
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Current.Shutdown(-1);
                }
            });

        }

        /*
        private void TestLoop()
        {
            async Task delayFun()
            {
                Debug.WriteLine("Starting delay");
                await Task.Delay(2000);
                Debug.WriteLine("Stopping delay");
            }
            Task.Run(async () =>
            {

                try
                {
                    while (true)
                    {
                        Debug.WriteLine("adding delay");
                        InputQueue.Add(delayFun);
                        await Task.Delay(2000);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Current.Shutdown(-1);
                }
            });

        }
        */
        private Vector GetPageSize() => new Vector(this.ActualWidth, this.ActualHeight - 20) * Renderer.WindowScale;
    }

}
