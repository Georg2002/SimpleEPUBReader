using EPUBRenderer;
using EPUBParser;
using System.Windows;
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;

namespace EPUBReader
{
    public static class ViewerInteracter
    {
        public static Viewer Viewer;
        public static Epub CurrentEpub;
        public static bool IsVertical;
        public static bool RTL;
        public static event EventHandler PageChanged;
        public static Color MarkingColor;

        private static void InvokePageChanged()
        {
            PageChanged.Invoke(null, null);
        }

        public static void Open(string Path)
        {
            CurrentEpub = new Epub(Path);
            if (CurrentEpub.Pages.Count != 0)
            {
                //just code to prevent crashes from unfinished
                //parts I don't want to finish right now
                RTL = CurrentEpub.Settings.RTL = true;
                IsVertical = CurrentEpub.Settings.Vertical = true;

                Viewer.SetToEpub(CurrentEpub);
            }
            LibraryManager.TryAddBook(CurrentEpub);
            InvokePageChanged();
        }

        internal static int GetCurrentPage()
        {
            return Viewer.CurrentPageNumber;
        }

        internal static int GetTotalPages()
        {
            return Viewer.TotalPageCount;
        }

        internal static double GetCurrentRenderPageRatio()
        {
            return Viewer.GetRenderPageRatio();
        }

        internal static int GetCurrentRenderPage()
        {
            return Viewer.RenderPages.IndexOf(Viewer.CurrentRenderPage);
        }

        internal static string GetCurrentPath()
        {
            if (CurrentEpub == null)
            {
                return "";
            }
            else
            {
                return CurrentEpub.FilePath;
            }
        }

        internal static List<MarkingDefinition> GetAllMarkings()
        {
            return Marker.GetAllMarkings(Viewer.RenderPages);
        }

        public static void SwitchLeft()
        {
            Viewer.SwitchLeft();
            InvokePageChanged();
        }

        public static void SwitchRight()
        {
            Viewer.SwitchRight();
            InvokePageChanged();
        }

        internal static void SetPage(int newPage)
        {
            Viewer.LoadPage(newPage);
            InvokePageChanged();
        }

        internal static void DeleteMarking(Point Pos)
        {
            var Marking = Marker.GetMarkingAt(Viewer.CurrentRenderPage, Pos);
            if (Marking != null)
            {
                Marker.DeleteMarking(Marking, Viewer.CurrentRenderPage);
            }
        }

        internal static void DragMark(Point Start, Point End)
        {
            if (Viewer.CurrentRenderPage == null) return;
            Marker.RemoveTempMarking();
            var Cmd = new MarkingCommand()
            {
                Color = new SolidColorBrush(MarkingColor),
                Page = Viewer.CurrentRenderPage,
                Pos1 = Start,
                Pos2 = End
            };
            Marker.MarkTemporarly(Cmd);
        }

        internal static void FinishDragMarking()
        {
            Marker.ApplyTempMarking();
        }

        internal static void SetNightmode(bool nightmode)
        {
            EPUBRenderer.GlobalSettings.SetNightmode(nightmode);
            Viewer.LoadPage();
        }

        internal static void LoadSave(SaveObject Save)
        {
            if (Save.LastBook != null)
            {
                LoadBookDefinition(Save.LastBook);
            }
            InvokePageChanged();
        }

        internal static void LoadBookDefinition(BookDefinition Book)
        {
            if (File.Exists(Book.FilePath))
            {
                Open(Book.FilePath);
                Viewer.LoadPageByRatio(Book.LastRenderPageIndex, Book.RenderPageRatio);
                Marker.ApplyAllMarkings(Book.Markings, Viewer.RenderPages);
            }
            InvokePageChanged();
        }

        internal static void LoadChapter(ChapterDefinition chapter)
        {
            string Source = chapter.Source;
            int n = CurrentEpub.Pages.FindIndex(a => a.FullName == Source);
            if (n == -1)
            {
                Logger.Report(string.Format("Chapter \"{0}\" not found",
                    Source), LogType.Error);
                return;
            }
            Viewer.LoadPage(n + 1, 1);
            InvokePageChanged();
        }
    }
}
