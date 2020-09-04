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
            PagePicker.Background = CloseButton.Background;
            Panel.Background = CloseButton.Background;
            Selector.Background = CloseButton.Background;
            LibraryManager.Selector = Selector;
            ChapterManager.Selector = Selector;     
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new OpenFileDialog
            {
                Filter = "Epub files(.epub)|*.epub",
                Multiselect = false
            };
            if (Dialog.ShowDialog() == true)
            {
                ViewerInteracter.Open(Dialog.FileName);
                VerticalButton.IsChecked = ViewerInteracter.IsVertical;
                RTLButton.IsChecked = ViewerInteracter.RTL;
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

        private void ToggleDayNightClicked(object sender, RoutedEventArgs e)
        {
            SetDayNight(!GlobalSettings.Nightmode);
        }

        public void SetDayNight(bool Nightmode)
        {
            GlobalSettings.Nightmode = Nightmode;
            Style ButtonStyle;
            var Res = Application.Current.Resources;
            if (Nightmode)
            {
                ButtonStyle = (Style)Res["ButtonStyleNight"];
            }
            else
            {
                ButtonStyle = (Style)Res["ButtonStyleDay"];
            }
            GlobalSettings.CurrentButtonStyle = ButtonStyle;
            ViewerInteracter.SetNightmode(Nightmode);
            MainWindow.SetNightmode(Nightmode);
            PagePicker.SetNightmode(Nightmode);
            Selector.SetNightmode(Nightmode);
            SetNightmode(Nightmode);
        }

        private void SetNightmode(bool nightmode)
        {
            NightmodeButton.IsChecked = nightmode;
            var ButtonStyle = GlobalSettings.CurrentButtonStyle;
            OpenButton.Style = ButtonStyle;
            PagePicker.Background = CloseButton.Background;
            Panel.Background = CloseButton.Background;
            Selector.Background = CloseButton.Background;
        }

        private void SelectChapter(object sender, RoutedEventArgs e)
        {
            ChapterManager.ResetSelector();
            LibraryManager.ResetSelector();
            ChapterManager.SetSelector();
            Selector.Visibility = Visibility.Visible;
        }

        private void SelectPage(object sender, RoutedEventArgs e)
        {
            PagePicker.Visibility = Visibility.Visible;
        }

        private void OpenLibrary(object sender, RoutedEventArgs e)
        {
            ChapterManager.ResetSelector();
            LibraryManager.ResetSelector();
            LibraryManager.SetSelector();
            Selector.Visibility = Visibility.Visible;
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            int i = Array.IndexOf(GlobalSettings.MarkingColors, ViewerInteracter.MarkingColor);
            i++;
            if (i >= GlobalSettings.MarkingColors.Length) i = 0;
            ViewerInteracter.MarkingColor = GlobalSettings.MarkingColors[i];
            ColorButton.Background = new SolidColorBrush(ViewerInteracter.MarkingColor);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ColorButton.Background = new SolidColorBrush(ViewerInteracter.MarkingColor);
        }
    }
}
