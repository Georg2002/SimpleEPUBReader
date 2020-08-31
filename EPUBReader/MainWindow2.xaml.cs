using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private bool IsGoingDown = false;
        private readonly Storyboard MoveUp;
        private readonly Storyboard MoveDown;
        private Point MouseTouchdownPos;
        string StartupFile;
        private DateTime LastAnimate;
        private SaveObject Save;

        public MainWindow2(string[] args)
        {
            InitializeComponent();
            TlBar.MainWindow = this;
            MoveUp = (Storyboard)this.FindResource("MoveUp");
            MoveDown = (Storyboard)this.FindResource("MoveDown");
            ViewerInteracter.Viewer = Viewer;
            Save = Loader.Load();
            if (Save == null)
            {
                Logger.Report("save not found, falling back to standards", LogType.Error);
                return;
            }
            if (Save.LibraryBooks != null)
            {
                LibraryManager.Books = Save.LibraryBooks;
                LibraryManager.CheckBookList();
            }
            else
            {
                LibraryManager.Books = new List<BookDefinition>();
            }
            if (args.Length > 0)
            {
                string Path = args[0];
                if (Path.ToLower().EndsWith(".epub"))
                {
                    if (File.Exists(Path))
                    {
                        StartupFile = Path;
                    }
                }
            }
        }

        private void Base_MouseMove(object sender, MouseEventArgs e)
        {
            if (DateTime.Now.Subtract(LastAnimate).TotalMilliseconds > 50)
            {
                LastAnimate = DateTime.Now;
                double CheckHeight = 25;
                Point Pos = e.GetPosition(Base);
                if (IsGoingDown)
                {
                    CheckHeight = 50;
                }
                if (Pos.Y < CheckHeight && !IsGoingDown)
                {
                    IsGoingDown = true;
                    MoveUp.Stop();
                    MoveDown.Begin();
                }
                else if (Pos.Y > CheckHeight && IsGoingDown && !LibraryManager.InUse)
                {
                    MoveDown.Stop();
                    MoveUp.Begin();
                    IsGoingDown = false;
                }
            }
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var Red = new SolidColorBrush(new Color() { R = 255, A = 50 });
                ViewerInteracter.DragMark(e.GetPosition(Viewer), MouseTouchdownPos, Red);
            }
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            ViewerInteracter.SwitchLeft();
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            ViewerInteracter.SwitchRight();
        }

        private void Base_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Saver.Save();
        }

        private void Base_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            MouseTouchdownPos = e.GetPosition(Viewer);
        }

        private void Base_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewerInteracter.DeleteMarking(e.GetPosition(Viewer));
        }

        private void Base_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewerInteracter.FinishDragMarking();
        }

        private void Base_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(StartupFile))
            {
                ViewerInteracter.Open(StartupFile);
            }

            if (Save != null)
            {
                if (Save.Nightmode)
                {
                    TlBar.SetDayNight(Save.Nightmode);
                }
                if (string.IsNullOrEmpty(StartupFile))
                {
                    ViewerInteracter.LoadSave(Save);
                }
            }
        }

        public void SetNightmode(bool Nightmode)
        {
            Style ButtonStyle;
            Color BackColor;
            if (Nightmode)
            {
                BackColor = (Color)ColorConverter.ConvertFromString("#121212");
                ButtonStyle = (Style)Resources["ButtonStyleNight"];
            }
            else
            {
                ButtonStyle = (Style)Resources["ButtonStyleDay"];
                BackColor = Colors.White;
            }
            Background = new SolidColorBrush(BackColor);
            ButtonLeft.Style = ButtonStyle;
            ButtonRight.Style = ButtonStyle;
        }
    }
}
