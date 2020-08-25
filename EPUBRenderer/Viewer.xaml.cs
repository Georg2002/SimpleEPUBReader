using EPUBParser;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
                public List<Marking> Markings;
        private Vector MarkingStartPos;
        private GestureHandler GestureHandler;

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
            CharSum = CharSum + 1;
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
            for (int i = 0; i < Pages.Count; i++)
            {
                var Page = Pages[i];               
                if ((PageCharIndex != -1 && PageCharIndex < Page.CharStartIndex) ||
                    (PageImgIndex != -1 && PageImgIndex < Page.ImageStartIndex))
                {
                    PageNumber = i;
                    break;
                }
            }
            CurrentPageNumber = PageNumber;
        }

        public Viewer()
        {
            InitializeComponent();
            Pages = new List<PageRenderer>();
            Markings = new List<Marking>();
            GestureHandler = new GestureHandler();
            GestureHandler.RelativeElement = this;
        }

        public void RefreshSize()
        {
            SetToEpub(epub, Markings);
            SetPageByIndex(PageCharIndex, PageImgIndex);
        }

        public void SwitchRight()
        {
            Switch(1);
        }

        public void SwitchLeft()
        {
            Switch(-1);
        }

        private void Switch(int Direction)
        {
            if (epub.Settings.RTL)
            {
                Direction *= -1;
            }
            CurrentPageNumber += Direction;
        }

        public void SetToEpub(Epub epub, List<Marking> Markings)
        {
            Pages.Clear();
            this.epub = epub;
            this.Markings = Markings;
            if (Width == 0 || Height == 0)
            {
                return;
            }
            foreach (var Page in epub.Pages)
            {
                Pages.AddRange(ChapterPagesCreator.GetRenderPages(Page, new Vector(Width - 30, Height - 30)));
            }
            int CharSum = 0;
            int ImageSumLast = 0;
            int CharSumLast = 0;
            foreach (var Page in Pages)
            {
                CharSum += Page.TextParts.Count;
                var MarkingsInPage = Markings.Where(a =>
                (a.CharEndIndex < CharSum && a.CharEndIndex >= CharSumLast) ||
                (a.CharStartIndex >= CharSumLast && a.CharStartIndex < CharSum));
                foreach (var Marking in MarkingsInPage)
                {
                    Page.Markings.Add(new Marking(Marking.CharStartIndex - CharSumLast, Marking.CharEndIndex - CharSumLast, Marking.Color));
                }
                Page.CharStartIndex = CharSumLast;
                Page.ImageStartIndex = ImageSumLast;
                ImageSumLast += Page.Images.Count;
                CharSumLast = CharSum;
            }
        }      

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            GestureHandler.CheckActivation();
            var MousePosPoint = Mouse.GetPosition(this);
            var MousePos = new Vector(MousePosPoint.X, MousePosPoint.Y);
            var Page = (PageRenderer)ContentGrid.Children[0];

            if (GestureHandler.SwipeLeft.Activated )
            {
                SwitchLeft();
            }
            if (GestureHandler.SwipeRight.Activated)
            {
                SwitchRight();
            }
            if (GestureHandler.Mark.Activated)
            {                    
                if (GestureHandler.Mark.Changed)
                {
                    MarkingStartPos = MousePos;
                }
                Page.DrawTemporaryMarking(MarkingStartPos, MousePos);
            }
            else
            {
                if (GestureHandler.Mark.Changed)
                {
                    Page.AddMarking(MarkingStartPos, MousePos);
                    var PageNewMarking = Page.Markings.Last();
                    var NewMarking = new Marking(Page.CharStartIndex + PageNewMarking.CharStartIndex,
                        Page.CharStartIndex + PageNewMarking.CharEndIndex, PageNewMarking.Color);
                    Markings.Add(NewMarking);
                }
            }
        }
    }
}
