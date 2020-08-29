using EPUBRenderer;
using EPUBParser;

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
    }
}
