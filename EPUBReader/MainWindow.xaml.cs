using EPUBParser;
using EPUBRenderer2;
using System.Windows.Threading;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Media;

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
             Viewer.SwitchLeft();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Viewer.SwitchRight();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            epub = new Epub(@"D:\Informatik\EPUBReader\TestResources\Index4.epub");
            var Red = new SolidColorBrush(new Color() { R = 255, A = 50 });
            Viewer.SetToEpub(epub);
            Viewer.CurrentPageNumber = 0;
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
