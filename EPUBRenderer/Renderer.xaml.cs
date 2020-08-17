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
                SetToPage(value);
            }
        }

        public int CurrentPage;

        public int PageCount { get => Pages.Count; }

        public List<PageRenderer> Pages;

        public Renderer()
        {
            InitializeComponent();
            Pages = new List<PageRenderer>();

            var Page = new EpubPage(new ZipEntry()
            { Content = File.ReadAllBytes(@"D:\Informatik\EPUBReader\TestResources\Index4\OPS\xhtml\0030.xhtml") }, new EpubSettings(), null);
            var Parts = new List<LinePart>();
            Page.Lines.ForEach(a => Parts.AddRange(a.Parts));




        }

        public void SetPage(int Page)
        {
            CurrentPage = Page;
            Content = Pages[Page];
        }

        private void SetToPage(EpubPage Page)
        {
            var CurrentPos = new ChapterPosition();
            var MaxPos = new ChapterPosition(Page.Lines.Count - 1,
                Page.Lines.Last().Parts.Count - 1,
                Page.Lines.Last().Parts.Last().Text.Length - 1);

            while (CurrentPos < MaxPos)
            {
                var NewPage = new PageRenderer();
                NewPage.StartPos = CurrentPos;

                CurrentPos = NewPage.EndPos;
            }
        }
    }

    public struct ChapterPosition
    {
        public int LineIndex;
        public int PartIndex;
        public int CharIndex;

        public ChapterPosition(int Line, int Part, int Char)
        {
            LineIndex = Line;
            PartIndex = Part;
            CharIndex = Char;
        }

        public static bool operator <(ChapterPosition a, ChapterPosition b)
        {
            if (a.LineIndex < b.LineIndex)
            {
                return true;
            }
            else if (a.LineIndex == b.LineIndex && a.PartIndex < b.PartIndex)
            {
                return true;
            }
            else if (a.LineIndex == b.LineIndex && a.PartIndex == b.PartIndex && a.CharIndex < b.CharIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ChapterPosition))
            {
                return this == (ChapterPosition)obj;
            }
            else
            {
                return false;
            }
           
        }

        public static bool operator >(ChapterPosition a, ChapterPosition b)
        {
            return !(a == b || a < b);
        }

        public static bool operator !=(ChapterPosition a, ChapterPosition b)
        {
            return !(a == b);
        }

        public static bool operator ==(ChapterPosition a, ChapterPosition b)
        {
            return a.LineIndex == b.LineIndex && a.PartIndex == b.PartIndex && a.CharIndex == b.CharIndex;
        }
    }
}
