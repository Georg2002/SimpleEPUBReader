using EPUBParser;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EPUBRenderer
{
    /// <summary>
    /// Interaction logic for PageRenderer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        private int _CurrentPage;
        public int CurrentPage
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

            if (PageCount != 0)
            {
                var Page = Pages[PageNumber - 1];
                Content = Page;
            }
        }
     
        public Viewer()
        {
            InitializeComponent();
            Pages = new List<PageRenderer>();
        }



        public void SetToEpub(Epub epub)
        {           
                foreach (var Page in epub.Pages)
                {
                    Pages.AddRange(ChapterPagesCreator.GetRenderPages(Page, new Vector(ActualWidth,ActualHeight)));
                }           
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var epub = new Epub(@"D:\Informatik\EPUBReader\TestResources\Index4.epub");
            SetToEpub(epub);
            CurrentPage = 19;
        }
    }
}
