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
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    public partial class ToolBar : UserControl
    {
        public MainWindow2 MainWindow;

        public ToolBar()
        {
            InitializeComponent();
            Panel.Background = CloseButton.Background;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new OpenFileDialog();
            Dialog.Filter = "Epub files(.epub)|*.epub";
            Dialog.Multiselect = false;
            if (Directory.Exists(GlobalSettings.LastDirectory))
            {
                Dialog.InitialDirectory = GlobalSettings.LastDirectory;
            }
            if (Dialog.ShowDialog() == true)
            {
                ViewerInteracter.Open(Dialog.FileName);
                VerticalButton.IsChecked = ViewerInteracter.IsVertical;
                RTLButton.IsChecked = ViewerInteracter.RTL;
                GlobalSettings.LastDirectory = Directory.GetParent(Dialog.FileName).FullName;
            }
        }

        private void CloseProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleFullscreen(object sender, RoutedEventArgs e)
        {
            if (MainWindow.WindowState == WindowState.Maximized)
            {
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                MainWindow.WindowStyle = WindowStyle.None;
                MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void ToggleDayNight(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Nightmode = !GlobalSettings.Nightmode;
            ViewerInteracter.SetNightmode(GlobalSettings.Nightmode);
            MainWindow.SetNightmode(GlobalSettings.Nightmode);
            SetNightmode(GlobalSettings.Nightmode);
        }

        private void SetNightmode(bool nightmode)
        {
            Style ButtonStyle;
            NightmodeButton.IsChecked = nightmode;
            if (nightmode)
            {
                ButtonStyle = (Style)Resources["ButtonStyleNight"];
            }
            else
            {
                ButtonStyle = (Style)Resources["ButtonStyleDay"];
            }
            OpenButton.Style = ButtonStyle;
            LibraryButton.Style = ButtonStyle;
            RTLButton.Style = ButtonStyle;
            VerticalButton.Style = ButtonStyle;
            NightmodeButton.Style = ButtonStyle;
            FullscreenButton.Style = ButtonStyle;
            CloseButton.Style = ButtonStyle;
            Panel.Background = CloseButton.Background;
        }
    }
}
