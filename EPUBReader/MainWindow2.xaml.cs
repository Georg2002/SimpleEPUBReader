﻿using System;
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
        string StartupFile;
        private DateTime LastAnimate;
        private SaveObject Save;
        private Point MouseTouchdownPos;
        private DateTime MouseTouchdown;
        private DateTime LastMouseMove;
        private Point LastMousePos;
        private bool GestureSwitched;

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
            if (Save.Fullscreen)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            if (Save.MarkingColor.Equals(new Color()))
            {
                ViewerInteracter.MarkingColor = GlobalSettings.MarkingColors[0];
            }
            else
            {
                ViewerInteracter.MarkingColor = Save.MarkingColor;
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
            Point MousePos = e.GetPosition(Viewer);
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
                else if (Pos.Y > CheckHeight && IsGoingDown && !GlobalSettings.LeaveMenuDown)
                {
                    MoveDown.Stop();
                    MoveUp.Begin();
                    IsGoingDown = false;
                }
            }
            DateTime Now = DateTime.Now;
            double Delta = LastMouseMove.Subtract(Now).TotalSeconds;
            double SinceTouchdown = Now.Subtract(MouseTouchdown).TotalSeconds;
            double Speed = (MousePos.X - LastMousePos.X) / Delta;
            if (Mouse.LeftButton == MouseButtonState.Pressed && !GestureSwitched)
            {
                if (Math.Abs(Speed) > 2000 && SinceTouchdown > 0.01 && SinceTouchdown < 0.1)
                {
                    GestureSwitched = true;
                    if (Speed > 0)
                    {
                        ViewerInteracter.SwitchRight();
                    }
                    else
                    {
                        ViewerInteracter.SwitchLeft();
                    }
                }
                else if (SinceTouchdown > 0.1)
                {
                    ViewerInteracter.DragMark(MousePos, MouseTouchdownPos);
                }
            }
            LastMousePos = MousePos;
            LastMouseMove = Now;
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
            MouseTouchdown = DateTime.Now;
            GestureSwitched = false;
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
