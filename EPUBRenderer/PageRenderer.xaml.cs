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
            TestLinePart.Part = new TextLinePart("III", "III");
            TestLinePart2.Part = new TextLinePart("これはテストです", "RubyTest");
            TestLinePart3.Part = new TextLinePart("試験", "しけん");
        }

        private void SetToPage(EpubPage Page)
        {

        }
    }
}
