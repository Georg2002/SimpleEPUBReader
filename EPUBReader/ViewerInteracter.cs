using EPUBRenderer;
using EPUBParser;
using System.Windows;
using System;
using System.Windows.Media;

namespace EPUBReader
{
    public static class ViewerInteracter
    {
        public static Viewer Viewer;
        private static Epub CurrentEpub;
        public static bool IsVertical;
        public static bool RTL;
        public static int TotalPageCount;
        public static int CurrentPage;

        public static void Open(string Path)
        {
            CurrentEpub = new Epub(Path);

            //just code to prevent crashes from unfinished
            //parts I don't want to finish right now
            RTL = CurrentEpub.Settings.RTL = true;
            IsVertical = CurrentEpub.Settings.Vertical = true;

            Viewer.SetToEpub(CurrentEpub);
            TotalPageCount = Viewer.TotalPageCount;
            CurrentPage = Viewer.CurrentPageNumber;
        }

        public static void SwitchLeft()
        {
            Viewer.SwitchLeft();
        }

        public static void SwitchRight()
        {
            Viewer.SwitchRight();
        }

        internal static void DeleteMarking(Point Pos)
        {
            var Marking = Marker.GetMarkingAt(Viewer.CurrentRenderPage, Pos);
            if (Marking != null)
            {
                Marker.DeleteMarking(Marking, Viewer.CurrentRenderPage);
            }
        }

        internal static void DragMark(Point Start, Point End, Brush Color)
        {
            if (Viewer.CurrentRenderPage == null) return;
            Marker.RemoveTempMarking();
            var Cmd = new MarkingCommand()
            {
                Color = Color,
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
    }
}
