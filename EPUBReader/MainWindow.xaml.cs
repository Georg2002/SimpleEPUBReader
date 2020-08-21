using EPUBRenderer;
using System.Windows;

namespace EPUBReader
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
             Viewer.CurrentPage++;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Viewer.CurrentPage--;
        }
    }
}
