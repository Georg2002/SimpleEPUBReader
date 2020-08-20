using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EPUBParser;

namespace EPUBRenderer
{
    /// <summary>
    /// Interaction logic for PageRenderer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        //separate images into new lines, if they aren't already
        //merge
        private EpubPage _EpubPage;
        public EpubPage Page
        {
            get
            {
                return _EpubPage;
            }
            set
            {
                _EpubPage = value;
                AddPages(value);
            }
        }

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

        public List<PageRenderer2> Pages;

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
            var Page = Pages[PageNumber - 1];
            Content = Page;
            _CurrentPage = PageNumber;
            Page.InvalidateVisual();
        }

        public Viewer()
        {
            InitializeComponent();
            Pages = new List<PageRenderer2>();

            var epub = new Epub(@"D:\Informatik\EPUBReader\TestResources\Index4.epub");
            foreach (var Page in epub.Pages)
            {
                Pages.AddRange(ChapterPagesCreator.GetRenderPages(Page, 1000, 700));
            }
            CurrentPage = 40;
        }
    }
}
