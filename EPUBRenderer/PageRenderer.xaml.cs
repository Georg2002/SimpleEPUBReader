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
    public partial class PageRenderer : UserControl
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

        public PageRenderer()
        {
            InitializeComponent();
            ScrollViewer.ScrollToRightEnd();
            var Page = new EpubPage(new TextFile(new ZipEntry()
            { Content = File.ReadAllBytes(@"D:\Informatik\EPUBReader\TestResources\Index4\OPS\xhtml\0030.xhtml") }), new EpubSettings());
            var Parts = new List<LinePart>();
            Page.Lines.ForEach(a => Parts.AddRange(a.Parts));
            foreach (var Part in Parts)
            {
                var NewPart = new RenderLinePart();
                NewPart.Part = Part;
                ItemsControl.Items.Add(NewPart);
            }
        }

        private void SetToPage(EpubPage Page)
        {

        }
    }
}
