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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EPUBRenderer3;
using Microsoft.Win32;

namespace EPUBReader2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MouseManager MouseManager;
        public byte ColorIndex;
        private const byte Alpha = 100;
        readonly Brush[] MarkingColors = new Brush[] {
            new SolidColorBrush(new Color() { R = 255, G = 0, B = 0, A = Alpha }),
            new SolidColorBrush( new Color() { R = 0, G = 255,B = 0,A = Alpha}),
            new SolidColorBrush(new Color() { R = 255, G = 255,B = 0,A = Alpha}),
            new SolidColorBrush(new Color() {R = 0, G = 0,B = 255,A = Alpha})
        };

        public MainWindow()
        {
            InitializeComponent();
            MouseManager = new MouseManager(Bar, ContentGrid, Renderer, this);
            Renderer.MarkingColors = MarkingColors;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //C:\Users\georg\Desktop\b\Zeug\a\Learning\Books\日常\[ìýüEèG_éáéþé¯ é»éóéóé┐ & Æÿ_ê╔ôñ ò¢É¼] ô·ÅÝé╠ë─ïxé¦.epub
            //D:\Informatik\EPUBReader\TestResources\DanMachi.epub
            //D:\Informatik\EPUBReader\TestResources\星界の紋章第一巻.epub
            Renderer.LoadBook(@"D:\Informatik\EPUBReader\TestResources\星界の紋章第一巻.epub", new PosDef(30, 0, 0, 0));
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Renderer.Switch(-1);
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Renderer.Switch(1);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            MouseManager.MouseMove(Mouse.GetPosition(this), Mouse.LeftButton == MouseButtonState.Pressed, Mouse.RightButton == MouseButtonState.Pressed);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new OpenFileDialog
            {
                Filter = "Epub files(.epub)|*.epub",
                Multiselect = false
            };
            if (Dialog.ShowDialog() == true)
            {
                Renderer.LoadBook(Dialog.FileName);
            }
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Chapter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Pages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStyle.None == WindowStyle)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
