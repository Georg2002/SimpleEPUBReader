using EPUBParser;
using EPUBRenderer;
using System.Windows.Threading;
using System.Windows;
using System;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {      
        private Epub epub;
        private DispatcherTimer ResizeTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            ResizeTimer = new DispatcherTimer();
            ResizeTimer.Interval = TimeSpan.FromMilliseconds(100);
            ResizeTimer.Tick += ChangeSize;      
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {           
             Viewer.CurrentPageNumber++;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Viewer.CurrentPageNumber--;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            epub = new Epub(@"D:\Informatik\EPUBReader\TestResources\Index4.epub");
            Viewer.SetToEpub(epub);       
        }

        private void ChangeSize(object sender, EventArgs e)
        {
            Viewer.RefreshSize();
            ResizeTimer.Stop();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeTimer.Stop();
            ResizeTimer.Start();
        }
    }
}
