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
using EPUBRenderer3;

namespace EPUBReader2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Renderer.LoadBook(@"D:\Informatik\EPUBReader\TestResources\星界の紋章第一巻.epub", new PosDef(30,0,0,0));
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Renderer.Switch(-1);
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Renderer.Switch(1);
        }
    }
}
