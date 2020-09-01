using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader
{
    public static class ChapterManager
    {
        public static ListSelector Selector;
        private static TocInfo toc;

        public static void LoadSelected(object sender, EventArgs args)
        {
            int i = Selector.SelectedIndex;
            if (i >= 0 && i < toc.Chapters.Count)
            {
                ViewerInteracter.LoadChapter(toc.Chapters[i]);             
            }          
        }

        public static void SetSelector()
        {
            Selector.DeleteMenu.IsEnabled = false;
            toc = ViewerInteracter.CurrentEpub.toc;
            var Titles = new List<string>();
            Selector.ItemSelected += LoadSelected;

            foreach (var Chapter in toc.Chapters)
            {
                Titles.Add(Chapter.Title);
            }
            Selector.ShownList = Titles;
        }

        public static void ResetSelector()
        {
            Selector.ItemSelected -= LoadSelected;
        }
    }
}
