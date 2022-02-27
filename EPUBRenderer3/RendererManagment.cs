﻿using System;
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
        private bool Rerender = false;


        public Renderer()
        {
            SizeChanged += Renderer_SizeChanged;
            MinHeight = 100;
            MinWidth = 100;
        }

        public void MoveSelection(int front, int end)
        {
            if (SelectionEnd == PosDef.InvalidPosition || SelectionStart == PosDef.InvalidPosition)
            {
                return;
            }
            RemoveSelection();
            if (SelectionStart > SelectionEnd)
            {
                var X = SelectionStart;
                SelectionStart = SelectionEnd;
                SelectionEnd = X;
            }
            var EndOld = SelectionEnd;
            var StartOld = SelectionStart;
            var Lines = CurrBook.PageFiles[SelectionStart.FileIndex].Lines;
            MoveSelectionPoints(front, end, Lines);

            Letter StartLetter = CurrBook.GetLetter(SelectionStart);
            Letter EndLetter = CurrBook.GetLetter(SelectionEnd);
            if (StartLetter == null || EndLetter == null)
            {
                SelectionEnd = EndOld;
                SelectionStart = StartOld;
            }
            //revert if overtook
            if (SelectionStart > SelectionEnd)
            {
                SelectionStart = StartOld;
                SelectionEnd = EndOld;
            }
            CurrBook.AddSelection(SelectionStart, SelectionEnd);
            Refresh();
        }

        private void MoveSelectionPoints(int front, int end, List<Line> Lines)
        {
            var EndOld = SelectionEnd;
            var StartOld = SelectionStart;
            if (front > 0) SelectionStart.Increment(Lines);
            else if (front < 0) SelectionStart.Decrement(Lines);
            if (SelectionStart.FileIndex == -1)
            {
                SelectionStart = StartOld;
                return;
            }
            if (end > 0) SelectionEnd.Increment(Lines);
            else if (end < 0) SelectionEnd.Decrement(Lines);
            if (SelectionEnd.FileIndex == -1)
            {
                SelectionEnd = EndOld;
                return;
            }

            Letter StartLetter = CurrBook.GetLetter(SelectionStart);
            Letter EndLetter = CurrBook.GetLetter(SelectionEnd);

            if (StartLetter == null || EndLetter == null)
            {
                return;
            }
            if (StartLetter.Type == LetterTypes.Letter && EndLetter.Type == LetterTypes.Letter)
            {
                var StartTL = (TextLetter)StartLetter;
                var EndTL = (TextLetter)EndLetter;
                if (!StartTL.IsRuby && !EndTL.IsRuby)
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
            Rerender = true;
        }

        public void LoadBook(string Path, DateTime DateAdded, PosDef Position = new PosDef(), List<MrkDef> Markings = null)
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path) || !Path.ToLower().EndsWith(".epub"))
            {
                MessageBox.Show($"Path {Path} invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SelectionStart = PosDef.InvalidPosition;
            SelectionEnd = PosDef.InvalidPosition;
            Markings = Markings ?? new List<MrkDef>();
            Epub epub = new Epub(Path);
            CurrBook = new RenderBook(epub, DateAdded);
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
            Refresh();
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
                CurrBook.CurrPos = FirstHit;
                Valid = FirstHit.FileIndex != -1;
            }
            return Valid;
        }

        public void DrawTempMarking(Point relPoint, byte ColorIndex)
        {
            CurrBook.RemoveMarking(FirstHit, SecondHit);
            SecondHit = ShownPage.Intersect(relPoint);
            CurrBook.CurrPos = SecondHit;
            CurrBook.AddMarking(FirstHit, SecondHit, ColorIndex);
            Refresh();
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
                CurrBook.CurrPos =Hit;
                if (Hit.FileIndex == -1) return;
                PosDef A;
                PosDef B;
                (A, B) = CurrBook.GetConnectedMarkings(Hit, ShownPage);
                CurrBook.RemoveMarking(A, B);
            }
            Refresh();
        }

        public bool StartSelection(Point relPoint)
        {
            bool Valid = false;
            if (CurrBook != null)
            {
                var NewStart = ShownPage.Intersect(relPoint);
                Valid = NewStart.FileIndex != -1;
                if (Valid) SelectionStart = NewStart;                          
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
            Refresh();
            RemoveSelection();
            SelectionEnd = ShownPage.Intersect(relPoint);
            CurrBook.CurrPos = SelectionEnd;
            if (SelectionStart != PosDef.InvalidPosition && SelectionEnd != PosDef.InvalidPosition)
            {
                CurrBook.AddSelection(SelectionStart, SelectionEnd);
            }
        }

        public string GetSelection() => CurrBook.GetSelection(SelectionStart, SelectionEnd);
      
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

        public void DeactivateSelection()
        {
            RemoveSelection();
            SelectionStart = PosDef.InvalidPosition;
            SelectionEnd = PosDef.InvalidPosition;
        }

        private void Refresh()
        {
            Rerender = true;
            InvalidateVisual();
        }
    }
}
