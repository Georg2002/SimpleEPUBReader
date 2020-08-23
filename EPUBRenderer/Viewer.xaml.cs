using EPUBParser;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace EPUBRenderer
{
    /// <summary>
    /// Interaction logic for PageRenderer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        private Epub epub;
        private int PageCharIndex;
        private int PageImgIndex;

        private int _CurrentPage;
        public int CurrentPageNumber
        {
            get => _CurrentPage;
            set
            {
                SetPage(value);
            }
        }

        public int PageCount { get => Pages.Count; }

        public List<PageRenderer> Pages;

        private void SetPage(int PageNumber)
        {
            if (PageNumber > PageCount)
            {
                PageNumber = PageCount;
            }
            if (PageNumber < 1)
            {
                PageNumber = 1;
            }
            _CurrentPage = PageNumber;

            PageRenderer Page = null;

            if (PageCount != 0)
            {
                Page = Pages[PageNumber - 1];

            }
            LoadNewUnloadOld(Page);
            SetPageIndexes();
        }

        private void LoadNewUnloadOld(PageRenderer New)
        {
            WasteGrid.Children.Clear();
            PageRenderer Old = null;
            if (ContentGrid.Children.Count != 0)
            {
                Old = (PageRenderer)ContentGrid.Children[0];
            }
            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(New);

            if (Old != New)
            {
                if (Old != null)
                {
                    WasteGrid.Children.Add(Old);
                    Old.Unload();
                }
            }
            else
            {
                Old = null;
            }
            New.Load();
        }

        private void SetPageIndexes()
        {
            int ImageSum = 0;
            int CharSum = 0;
            for (int i = 0; i < CurrentPageNumber - 1; i++)
            {
                var Page = Pages[i];
                ImageSum += Page.Images.Count;
                CharSum += Page.TextParts.Count;
            }

            var CurrentPage = Pages[CurrentPageNumber - 1];
            ImageSum = ImageSum + 1;
            CharSum = CharSum  + 1;
            if (CurrentPage.TextParts.Count == 0)
            {
                PageImgIndex = ImageSum - 1;
                PageCharIndex = -1;
            }
            else
            {
                PageImgIndex = -1;
                PageCharIndex = CharSum - 1;
            }
        }

        public void SetPageByIndex(int PageCharIndex, int PageImgIndex)
        {
            int PageNumber = 1;
            int CharSum = 0;
            int ImgSum = 0;
            for (int i = 0; i < Pages.Count; i++)
            {
                var Page = Pages[i];
                CharSum += Page.TextParts.Count;
                ImgSum += Page.Images.Count;
                if ((PageCharIndex != -1 && PageCharIndex < CharSum) ||
                    (PageImgIndex != -1 && PageImgIndex < ImgSum))
                {
                    PageNumber = i + 1;
                    break;
                }
            }
            CurrentPageNumber = PageNumber;
        }

        public Viewer()
        {
            InitializeComponent();
            Pages = new List<PageRenderer>();
        }

        public void RefreshSize()
        {
            SetToEpub(epub);
            SetPageByIndex(PageCharIndex, PageImgIndex);
        }

        public void SetToEpub(Epub epub)
        {
            Pages.Clear();
            this.epub = epub;
            if (Width == 0 || Height == 0)
            {
                return;
            }
            foreach (var Page in epub.Pages)
            {
                Pages.AddRange(ChapterPagesCreator.GetRenderPages(Page, new Vector(Width - 30, Height - 30)));
            }
        }
    }
}
