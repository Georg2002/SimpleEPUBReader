using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EPUBParser;

namespace EPUBRenderer3
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

        public Renderer()
        {
            SizeChanged += Renderer_SizeChanged;
            MinHeight = 100;
            MinWidth = 100;
        }

        public void MoveSelection(int front, int end)
        {
            RemoveSelection();
            var EndOld = SelectionEnd;
            var StartOld = SelectionStart;
            var Lines = CurrBook.PageFiles[CurrBook.CurrPos.FileIndex].Lines;
            MoveSelectionPoints(front, end, Lines);
            //revert if overtook
            if (SelectionStart > SelectionEnd)
            {
                SelectionStart = StartOld;
                SelectionEnd = EndOld;
            }
            CurrBook.AddSelection(SelectionStart, SelectionEnd);
            InvalidateVisual();
        }

        private void MoveSelectionPoints(int front, int end, List<Line> Lines)
        {
            var EndOld = SelectionEnd;
            var StartOld = SelectionStart;
            if (front > 0) SelectionStart.Increment(Lines);
            else if (front < 0) SelectionStart.Decrement(Lines);
            if (SelectionStart.FileIndex == -1) SelectionStart = StartOld;
            if (end > 0) SelectionEnd.Increment(Lines);
            else if (end < 0) SelectionEnd.Decrement(Lines);
            if (SelectionEnd.FileIndex == -1) SelectionEnd = EndOld;

            Letter StartLetter = CurrBook.GetLetter(SelectionStart);
            Letter EndLetter = CurrBook.GetLetter(SelectionEnd);
            if (StartLetter == null ||EndLetter == null)
            {
                SelectionEnd = EndOld;
                SelectionStart = StartOld;
                return;
            }
            if (StartLetter.Type == LetterTypes.Letter && EndLetter.Type == LetterTypes.Letter)
            {
                var StartTL = (TextLetter)StartLetter;
                var EndTL = (TextLetter)EndLetter;
                if (StartTL.FontSize == TextLetter.StandardFontSize && EndTL.FontSize == TextLetter.StandardFontSize)
                {
                    return;
                }
            }           
                MoveSelectionPoints(front, end, Lines);            
        }

        private void Renderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageSize = new Vector(ActualWidth, ActualHeight);
            if (CurrBook != null)
            {
                CurrBook.Position(PageSize);
                OpenPage(CurrBook.CurrPos);
            }
        }

        public void LoadBook(string Path, PosDef Position = new PosDef(), List<MrkDef> Markings = null)
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path) || !Path.ToLower().EndsWith(".epub"))
            {
                MessageBox.Show($"Path {Path} invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Markings = Markings ?? new List<MrkDef>();
            Epub epub = new Epub(Path);
            CurrBook = new RenderBook(epub);
            SetMarkings(Markings);
            CurrBook.Position(PageSize);
            OpenPage(Position);
        }

        private void SetMarkings(List<MrkDef> Markings)
        {
            CurrBook.SetMarkings(Markings);
        }

        public void OpenPage(PosDef Position)
        {
            if (CurrBook == null) return;
            CurrBook.CurrPos = Position;
            var PageFile = CurrBook.PageFiles[Position.FileIndex];
            ShownPage = PageFile.Pages.Find(a => a.Within(Position));
            InvalidateVisual();
        }

        public void Switch(int Dir)
        {
            if (CurrBook == null) return;
            int FileIndex = CurrBook.CurrPos.FileIndex;
            int PageIndex = CurrBook.PageFiles[FileIndex].Pages.IndexOf(ShownPage);
            PageIndex += Dir;
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
                    if (FileIndex >= CurrBook.PageFiles.Count)
                    {
                        FileIndex = CurrBook.PageFiles.Count - 1;
                        PageIndex = CurrBook.PageFiles[FileIndex].Pages.Count - 1;
                        break;
                    }
                    PageIndex -= CurrBook.PageFiles[FileIndex - 1].Pages.Count;
                }
            }
            OpenPage(CurrBook.PageFiles[FileIndex].Pages[PageIndex].StartPos);
        }

        public bool StartMarking(Point relPoint)
        {
            bool Valid = false;
            if (CurrBook != null)
            {
                FirstHit = ShownPage.Intersect(relPoint);
                Valid = FirstHit.FileIndex != -1;
            }
            return Valid;
        }

        public void DrawTempMarking(Point relPoint, byte ColorIndex)
        {
            CurrBook.RemoveMarking(FirstHit, SecondHit);
            SecondHit = ShownPage.Intersect(relPoint);
            CurrBook.AddMarking(FirstHit, SecondHit, ColorIndex);
            InvalidateVisual();
        }

        public void FinishMarking(Point relPoint, byte ColorIndex)
        {
            DrawTempMarking(relPoint, ColorIndex);
            SecondHit = new PosDef(-1, -1, -1, -1);
        }

        public void RemoveMarking(Point relPoint)
        {
            if (CurrBook != null)
            {
                PosDef Hit = ShownPage.Intersect(relPoint);
                if (Hit.FileIndex == -1) return;
                PosDef A;
                PosDef B;
                (A, B) = CurrBook.GetConnectedMarkings(Hit, ShownPage);
                CurrBook.RemoveMarking(A, B);
            }
            InvalidateVisual();
        }

        public bool StartSelection(Point relPoint)
        {
            bool Valid = false;
            if (CurrBook != null)
            {
                SelectionStart = ShownPage.Intersect(relPoint);
                Valid = SelectionStart.FileIndex != -1;
            }
            return Valid;
        }

        public void RemoveSelection()
        {
            CurrBook.RemoveSelection(SelectionStart, SelectionEnd);
        }

        public void ContinueSelection(Point relPoint)
        {
            InvalidateVisual();

            RemoveSelection();
            SelectionEnd = ShownPage.Intersect(relPoint);
            if (SelectionStart != PosDef.InvalidPosition && SelectionEnd != PosDef.InvalidPosition)
            {
                CurrBook.AddSelection(SelectionStart, SelectionEnd);
                if (SelectionEnd < SelectionStart)
                {
                    var x = SelectionEnd;
                    SelectionEnd = SelectionStart;
                    SelectionStart = x;
                }           
            }            
        }

        public string GetSelection()
        {
            return CurrBook.GetSelection(SelectionStart, SelectionEnd);
        }

        public int GetPageCount()
        {
            if (CurrBook == null) return 0;
            return CurrBook.GetPageCount();
        }

        public int GetCurrentPage()
        {
            if (CurrBook == null) return 0;
            return CurrBook.GetCurrentPage();
        }

        public List<string> GetChapters()
        {
            if (CurrBook == null) return new List<string>();
            return CurrBook.GetChapters();
        }

        public void SetChapter(int chapterIndex)
        {
            PosDef Pos = CurrBook.GetChapterPos(chapterIndex);
            OpenPage(Pos);
        }

        public LibraryBook GetCurrentBook()
        {
            if (CurrBook == null) return new LibraryBook() { CurrPos = PosDef.InvalidPosition };
            return CurrBook.GetLibraryBook();
        }
    }
}
