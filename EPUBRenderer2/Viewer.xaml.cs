using EPUBParser;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EPUBRenderer
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        public List<RenderPage> RenderPages;
        public Vector PageSize;
        private Epub epub;
        public RenderPage CurrentRenderPage
        {
            get => Renderer.Page;
        }

        public Viewer()
        {
            InitializeComponent();
            Marker.Renderer = Renderer;
        }

        public int TotalPageCount;
        public int CurrentPageNumber;

        public void SetToEpub(Epub epub)
        {
            this.epub = epub;
            PageSize = new Vector(1000, 700);
            var PageLines = new List<List<EpubLine>>();
            epub.Pages.ForEach(a => PageLines.Add(WordSplitter.SplitIntoWords(a)));
            RenderPages = new List<RenderPage>();
            PageLines.ForEach(a => RenderPages.Add(new RenderPage() { TextElements = TextElementStringCreator.GetElements(a, epub.Settings.Vertical) }));
            CurrentPageNumber = 1;
            RefreshSize();
        }

        public void RefreshSize()
        {
            if (epub == null) return;
            TotalPageCount = 0;
            PageSize = new Vector(ActualWidth, ActualHeight);
            Renderer.PageSize = PageSize;
            int OldInnerPageCount = 0;
            if (Renderer.Page != null)
            {
                OldInnerPageCount = Renderer.Page.PageCount;
            }
            RenderPages.ForEach(a =>
        {
            TextPositioner.Position(a.TextElements, PageSize, epub.Settings);
            PageSetter.SetPageDefinitions(a, PageSize);
            TotalPageCount += a.PageCount;
        });
            if (Renderer.Page != null)
            {
                int RenderPageIndex = RenderPages.IndexOf(Renderer.Page);
                double Ratio = (double)(Renderer.Page.CurrentPage - 1) / (double)OldInnerPageCount;
                int NewInnerPageNumber = (int)Math.Round(Renderer.Page.PageCount * Ratio) + 1;
                if (NewInnerPageNumber > Renderer.Page.PageCount)
                {
                    NewInnerPageNumber = Renderer.Page.PageCount;
                }
                LoadPage(RenderPageIndex + 1, NewInnerPageNumber);
            }
            else
            {
                LoadPage(1);
            }
        }

        private void LoadPage(int RenderPageNumber, int InnerPageNumber)
        {
            Renderer.Page = RenderPages[RenderPageNumber - 1];
            Renderer.Page.CurrentPage = InnerPageNumber;
            CurrentPageNumber = 0;
            for (int i = 0; i < RenderPageNumber - 1; i++)
            {
                CurrentPageNumber += RenderPages[i].PageCount;
            }
            CurrentPageNumber += Renderer.Page.CurrentPage;
            LoadPage();
        }

        public void LoadPage()
        {
            Renderer.InvalidateVisual();
        }

        public void LoadPage(int Number)
        {
            if (Number > TotalPageCount) CurrentPageNumber = TotalPageCount;
            else if (Number <= 0) CurrentPageNumber = 1;
            else CurrentPageNumber = Number;

            int Sum = 0;
            int LastSum = 0;
            for (int i = 0; i < TotalPageCount; i++)
            {
                var Page = RenderPages[i];
                Sum += Page.PageCount;
                if (CurrentPageNumber <= Sum)
                {
                    Renderer.Page = Page;
                    Renderer.Page.CurrentPage = CurrentPageNumber - LastSum;
                    break;
                }
                LastSum = Sum;
            }
            LoadPage();
        }

        public void SwitchLeft()
        {
            SwitchPage(-1);
        }

        public void SwitchRight()
        {
            SwitchPage(1);
        }

        private void SwitchPage(int Direction)
        {

            if (epub.Settings.RTL)
            {
                Direction *= -1;
            }
            LoadPage(CurrentPageNumber + Direction);
        }
    }
}
