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
        public Viewer()
        {
            InitializeComponent();
        }

        public int CurrentPageNumber { get; set; }

        public void SetToEpub(Epub epub)
        {
            throw new NotImplementedException();
        }

        public void SwitchLeft()
        {
            throw new NotImplementedException();
        }

        public void RefreshSize()
        {
            throw new NotImplementedException();
        }

        public void SwitchRight()
        {
            throw new NotImplementedException();
        }
    }
}
