using EPUBParser;
using EPUBRenderer;
using System.Windows.Threading;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {      
        private Epub epub;
        private readonly DispatcherTimer ResizeTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            ResizeTimer = new DispatcherTimer();
            ResizeTimer.Interval = TimeSpan.FromMilliseconds(5);
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
            Viewer.SetToEpub(epub);
            Viewer.LoadPage(1);
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var Red = new SolidColorBrush(new Color() { R = 255, A = 50 });
            var cmd = new MarkingCommand
            {
                Page = Viewer.CurrentRenderPage,
                Pos1 = new Point(1590, 10),
                Pos2 = new Point(1590, 40),
                Color = Red,
                RenderPageIndex = Viewer.RenderPages.IndexOf(Viewer.CurrentRenderPage)
            };
            Marker.MarkTemporarly(cmd);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var Marking = Marker.GetMarkingAt(Viewer.CurrentRenderPage, new Point(1590, 10));
            if (Marking != null)
            {
                Marker.DeleteMarking(Marking, Viewer.CurrentRenderPage);
            }
        }
    }
}
