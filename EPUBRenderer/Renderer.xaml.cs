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
    public partial class Renderer : UserControl
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
                _CurrentPage = value;
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
            var Page = Pages[PageNumber - 1];       
            Content = Page;
            Page.InvalidateVisual();
        }

        public Renderer()
        {
            InitializeComponent();
            Pages = new List<PageRenderer>();

            var epub = new Epub(@"D:\Informatik\EPUBReader\TestResources\Index4.epub");          
            foreach (var Page in epub.Pages)
            {
                AddPages(Page);
            }
            CurrentPage = 40;
        }

        private void AddPages(EpubPage Page)
        {
            var CurrentPos = new ChapterPosition();
            var MaxPos = new ChapterPosition(Page.Lines.Count - 1,
                Page.Lines.Last().Parts.Count - 1,
                Page.Lines.Last().Parts.Last().Text.Length - 1);

            while (CurrentPos < MaxPos)
            {
                var NewPage = new PageRenderer();
                NewPage.StartPos = CurrentPos;
                NewPage.PageHeight = 700;
                NewPage.PageWidth = 1000;
                NewPage.SetContent(Page);               
                Pages.Add(NewPage);              
                CurrentPos = NewPage.EndPos;              
            }
        }
    }
}
