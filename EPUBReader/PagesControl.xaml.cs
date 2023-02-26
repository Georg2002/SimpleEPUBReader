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

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for PagesControl.xaml
    /// </summary>
    public partial class PagesControl : UserControl
    {
        public MainWindow Main;


        public PagesControl()
        {
            InitializeComponent();
        }

        private void M100(object sender, RoutedEventArgs e)
        {
            Jump(-100);
        }

        private void M10(object sender, RoutedEventArgs e)
        {
            Jump(-10);
        }      

        private void M1(object sender, RoutedEventArgs e)
        {
            Jump(-1);
        }

        private void Finished(object sender, RoutedEventArgs e)
        {
            Main.FinishPageJump();
        }

        private void P1(object sender, RoutedEventArgs e)
        {
            Jump(1);
        }

        private void P10(object sender, RoutedEventArgs e)
        {
            Jump(10);
        }

        private void P100(object sender, RoutedEventArgs e)
        {
            Jump(100);
        }

        private void Jump(int Amount)
        {
            Main.JumpPages(Amount);
            Refresh();
        }

        public void Refresh()
        {
            TxtInidicator.Text = $"{Main.GetCurrentPage()}/{Main.GetPageCount()}";
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }
    }
}
