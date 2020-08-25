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

namespace EPUBRenderer2
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        List<RenderPage> Pages;
        public Vector PageSize;
        private Epub epub;

        public Viewer()
        {
            InitializeComponent();
        }

        public int CurrentPageNumber { get; set; }

        public void SetToEpub(Epub epub)
        {
            this.epub = epub;
            CurrentPageNumber = 21;
            PageSize = new Vector(1000, 700);
            var PageLines = new List<List<EpubLine>>();
            epub.Pages.ForEach(a => PageLines.Add(WordSplitter.SplitIntoWords(a)));
            Pages = new List<RenderPage>();
            PageLines.ForEach(a => Pages.Add(new RenderPage() { TextElements = TextElementStringCreator.GetElements(a, epub.Settings.Vertical) }));
            RefreshSize();
        }

        public void RefreshSize()
        {
            Pages.ForEach(a => TextPositioner.Position(a.TextElements, PageSize, epub.Settings));
            Renderer.PageSize = PageSize;
            LoadPage();
        }

        public void LoadPage()
        {
            Renderer.Page = Pages[CurrentPageNumber - 1];
            Renderer.InvalidateVisual();
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
            CurrentPageNumber += Direction;
            if (CurrentPageNumber < 1)
            {
                CurrentPageNumber = 1;
            }
            if (CurrentPageNumber > Pages.Count)
            {
                CurrentPageNumber = Pages.Count;
            }    
            LoadPage();
        }
    }
}
