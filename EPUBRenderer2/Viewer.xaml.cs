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
        private bool ResizeRunning;
        private double ResizePageRatio;

        public RenderPage CurrentRenderPage
        {
            get => Renderer.Page;
        }

        public Viewer()
        {
            InitializeComponent();
            Marker.Renderer = Renderer;
            RenderPages = new List<RenderPage>();
        }

        public int TotalPageCount;
        public int CurrentPageNumber;

        public void SetToEpub(Epub epub)
        {
            this.epub = epub;
            Renderer.Page = null;
            WritingDirectionModifiers.SetDirection(epub.Settings);
            PageSize = new Vector(1000, 700);
            var PageLines = new List<List<EpubLine>>();
            epub.Pages.ForEach(a => PageLines.Add(WordSplitter.SplitIntoWords(a)));
            RenderPages.Clear();
            PageLines.ForEach(a => RenderPages.Add(new RenderPage() { TextElements = TextElementStringCreator.GetElements(a, epub.Settings.Vertical) }));
            CurrentPageNumber = 1;
            ResizeRunning = false;
            RefreshSize();
        }

        public void RefreshSize()
        {
            if (epub == null) return;
            TotalPageCount = 0;
            if (PageSize.X == Math.Round( ActualWidth,0) && 
                PageSize.Y == Math.Round(ActualHeight)) return;
            PageSize = new Vector(ActualWidth, ActualHeight);
            Renderer.PageSize = PageSize;          
            if (Renderer.Page != null && !ResizeRunning)
            {
                ResizePageRatio = GetRenderPageRatio();
                ResizeRunning = true;
            }
            RenderPages.ForEach(a =>
        {
            if (a.TextElements.Count == 0) return;
            a.SinglePageOffset = WritingDirectionModifiers.GetPageOffset(PageSize);
            TextPositioner.Position(a.TextElements, PageSize);
            a.PageCount = WritingDirectionModifiers.GetPageCount(a, PageSize);
            TotalPageCount += a.PageCount;
        });
            if (Renderer.Page != null)
            {
                int RenderPageIndex = RenderPages.IndexOf(Renderer.Page);
                LoadPageByRatio(RenderPageIndex, ResizePageRatio);
            }
            else
            {
                LoadPage(1);
            }
        }

        public void LoadPageByRatio(int RenderPage, double Ratio)
        {          
            RenderPage Page = RenderPages[RenderPage];
            int NewInnerPageNumber = (int)Math.Round((Page.PageCount)* Ratio, MidpointRounding.AwayFromZero);
            if (NewInnerPageNumber > Page.PageCount)
            {
                NewInnerPageNumber = Page.PageCount;
            }
            if (NewInnerPageNumber == 0) NewInnerPageNumber = 1;
            LoadPage(RenderPage + 1, NewInnerPageNumber);
        }

        public double GetRenderPageRatio()
        {
            if (Renderer.Page != null)
            {
                return (double)(Renderer.Page.CurrentPage) / (double)(Renderer.Page.PageCount);
            }
            else
            {
                return 0;
            }
        }

        public void LoadPage(int RenderPageNumber, int InnerPageNumber)
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
            ResizeRunning = false;
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
            if (epub == null) return;
            if (epub.Settings.RTL)
            {
                Direction *= -1;
            }
            LoadPage(CurrentPageNumber + Direction);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshSize();
        }
    }
}
