using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using EPUBRenderer;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Interop;

namespace EPUBReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog Dialog = new OpenFileDialog()
        {
            Filter = "Epub files(.epub)|*.epub",
            Multiselect = false
        };
        private Library Library = new Library();
        private bool FunctionsLocked = false;
        private readonly MouseManager MouseManager;
        public byte ColorIndex = 1;

        //marking colors copied to c++ parts
        private const byte Alpha = 100;
        private bool DictionaryActive = false;
        readonly SolidColorBrush[] MarkingColors = new[] {null,
            new SolidColorBrush(new Color() { R = 255, G = 0, B = 0, A = Alpha }),
            new SolidColorBrush( new Color() { R = 0, G = 255,B = 0,A = Alpha}),
            new SolidColorBrush(new Color() { R = 255, G = 255,B = 0,A = Alpha}),
            new SolidColorBrush(new Color() {R = 0, G = 0,B = 255,A = Alpha})
        };
        private Vector WindowSize;
        private DispatcherTimer Timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            MouseManager = new MouseManager(Bar, ContentGrid, Renderer, this);
            Renderer.MarkingColors = MarkingColors;
            Bar.Margin = new Thickness(0, -MouseManager.BarHeight, 0, 0);
            ContentGrid.Margin = new Thickness(0, MouseManager.BarHeight / 2, 0, MouseManager.BarHeight / 2);
            Bar.Height = MouseManager.BarHeight;
            PagesControl.Main = this;
            Menu.Main = this;
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += timer_tick;
            Timer.Start();
        }

        private void timer_tick(object sender, EventArgs e) => txtTimer.Text = DateTime.Now.ToString("HH:mm");
        private void LoadSave(string arg)
        {
            SaveStruc Save = SaveAndLoad.LoadSave();
            ColorIndex = Save.ColorIndex != 0 && Save.ColorIndex < MarkingColors.Length ? Save.ColorIndex : (byte)1;
            ColorButton.Background = MarkingColors[ColorIndex];
            string Root = Path.GetPathRoot(Save.LastDirectory);
            if (!Directory.Exists(Root) || Root == "\\") Save.LastDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            else while (!Directory.Exists(Save.LastDirectory)) Save.LastDirectory = Path.Combine(Save.LastDirectory, @"\..");
            Dialog.InitialDirectory = Path.GetFullPath(Save.LastDirectory);
            if (Save.Books != null)
            {
                Library.SetFromSave(Save.Books);
                if (File.Exists(arg) && arg.ToLower().EndsWith(".epub"))
                {
                    int index = Library.GetIndex(arg);
                    if (index != -1) Save.CurrentBookIndex = index;
                    else
                    {
                        Renderer.LoadBook(arg, DateTime.Now);
                        SetTitle();
                    }
                }
                if (Save.CurrentBookIndex >= 0 && Save.CurrentBookIndex < Save.Books.Count && Renderer.CurrBook == null)
                {
                    SetToBook(Save.CurrentBookIndex);
                }
            }
            if (Save.WindowSize.X != 0 && Save.WindowSize.Y != 0)
            {
#if DEBUG
                Height = 750;
                Width = 1170;
#else
                if (Save.WindowSize.Y >= MinHeight && Save.WindowSize.X >= MinWidth)
                {
                    Height = Save.WindowSize.Y;
                    Width = Save.WindowSize.X;
                }
#endif
            }
            if (Save.Fullscreen) Fullscreen_Click(null, null);
            if (Save.DictOpen) Dict_Click(null, null);
        }

        internal void DeleteBook(int index)
        {
            Library.DeleteBook(index);
            Menu.SetToLibrary(Library.GetBooks());
        }

        internal int GetCurrentPage() => Renderer.GetCurrentPage();
        internal bool MouseOverText()
        {
            var Pos = Mouse.GetPosition(Renderer);
            return Renderer.ActualHeight >= Pos.Y && Renderer.ActualWidth >= Pos.X && Pos.X >= 0 && Pos.Y >= 0;
        }

        internal void SetToBook(int LibraryIndex)
        {
            LibraryBook Book = Library.GetBook(LibraryIndex);
            Renderer.LoadBook(Book.FilePath, Book.DateAdded, Book.CurrPos, Book.Markings);
            SetTitle();
            if (Menu.Visibility == Visibility.Visible) Library_Click(null, null);
        }

        internal void Lookup(string Text) => DictControl.SelectionChanged(Text);
        internal int GetPageCount() => Renderer.GetPageCount();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DictControl.Init(this);
            var Args = Environment.GetCommandLineArgs();
            string arg = Args.Length > 1 ? Args[1] : null;
            LoadSave(arg);
        }

        private void Right_Click(object sender, RoutedEventArgs e) => JumpPages(-1);
        private void Left_Click(object sender, RoutedEventArgs e) => JumpPages(1);
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    Right_Click(null, null);
                    break;
                case Key.Left:
                    Left_Click(null, null);
                    break;
                case Key.Tab:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            MouseManager.MouseMove(Mouse.GetPosition(this), Mouse.LeftButton == MouseButtonState.Pressed, Mouse.RightButton == MouseButtonState.Pressed);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsLocked) return;
            if (Dialog.ShowDialog() == true)
            {
                Library.AddOrReplaceBook(Renderer.GetCurrentBook());
                int bookIndex = Library.GetIndex(Dialog.FileName);
                if (bookIndex != -1)
                {
                    SetToBook(bookIndex);
                    return;
                }
                Renderer.LoadBook(Dialog.FileName, DateTime.Now);
                SetTitle();
                Dialog.InitialDirectory = Path.GetDirectoryName(Dialog.FileName);
            }
        }

        private void SetTitle()
        {

#if DEBUG
            const string PresetText = "Debug mode active, start window size preset";
            if (Renderer.CurrBook == null) Title = PresetText;
            else Title = Renderer.CurrBook.Title + "; " + PresetText;
#else
            Title = Renderer.CurrBook == null ? "Epub Reader 2" : Renderer.CurrBook.Title;
#endif
            txtTitle.Text = Title;
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Visibility == Visibility.Visible)
            {
                if (Menu.ShowingChapters) return;
                Menu.Visibility = Visibility.Collapsed;
                MouseManager.Locked = false;
                FunctionsLocked = false;
            }
            else
            {
                if (FunctionsLocked) return;
                MouseManager.Locked = true;
                MouseManager.MoveDown();
                FunctionsLocked = true;
                Menu.Visibility = Visibility.Visible;
                Library.AddOrReplaceBook(Renderer.GetCurrentBook());
                Menu.SetToLibrary(Library.GetBooks());
            }
        }

        private void Chapter_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Visibility == Visibility.Visible)
            {
                if (!Menu.ShowingChapters) return;
                Menu.Visibility = Visibility.Collapsed;
                MouseManager.Locked = false;
                FunctionsLocked = false;
            }
            else
            {
                if (FunctionsLocked) return;
                MouseManager.MoveDown();
                MouseManager.Locked = true;
                FunctionsLocked = true;
                Menu.Visibility = Visibility.Visible;
                Menu.SetToChapters(Renderer.GetChapters());
            }
        }

        public void SetChapter(int ChapterIndex)
        {
            Renderer.SetChapter(ChapterIndex);
            if (Menu.Visibility == Visibility.Visible) Chapter_Click(null, null);
        }

        private void Pages_Click(object sender, RoutedEventArgs e)
        {
            if (FunctionsLocked) return;
            MouseManager.Locked = true;
            PagesControl.Visibility = Visibility.Visible;
        }

        public void FinishPageJump()
        {
            PagesControl.Visibility = Visibility.Collapsed;
            MouseManager.Locked = false;
        }

        public void JumpPages(int Amount)
        {
            Renderer.Switch(Amount);
            PagesControl.Refresh();
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            ColorIndex++;
            if (ColorIndex == MarkingColors.Length) ColorIndex = 1;
            ColorButton.Background = MarkingColors[ColorIndex];
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.ToolWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private SaveStruc GetSave()
        {
            var Save = new SaveStruc();
            var Book = Renderer.GetCurrentBook();
            Library.AddOrReplaceBook(Book);
            Save.CurrentBookIndex = Library.GetIndex(Book);
            Save.Books = Library.GetBooks();
            Save.Fullscreen = WindowState == WindowState.Maximized;
            Save.ColorIndex = ColorIndex;
            Save.LastDirectory = Dialog.InitialDirectory;
            Save.WindowSize = WindowSize;
            Save.DictOpen = DictionaryActive;
            return Save;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PagesControl.Refresh();
            if (WindowState == WindowState.Normal)
            {
                WindowSize.X = ActualWidth;
                WindowSize.Y = ActualHeight;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => SaveAndLoad.Save(GetSave());
        private void Dict_Click(object sender, RoutedEventArgs e)
        {
            this.DictColumn.Width = this.DictionaryActive ? new GridLength(0, GridUnitType.Pixel) : new GridLength(1, GridUnitType.Auto);
            DictionaryActive = !DictionaryActive;
            DictControl.ActiveSet(DictionaryActive);
            MouseManager.DictActive = DictionaryActive;
            Renderer.DeactivateSelection();
        }

        internal void DictSelectionMoved(int front, int end)
        {
            Renderer.MoveSelection(front, end);
            DictControl.SelectionChanged(Renderer.GetSelection());
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            txtClose.Text = "Double\nclick";
            ResetCloseText();
        }

        private async void ResetCloseText()
        {
            await Task.Delay(2000);
            txtClose.Text = "Close";
        }

        private void Close_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Application.Current.Shutdown();
    }

    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    public class ThicknessConverter : IValueConverter
    {
        //spaghet
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness)) return new Thickness();
            return new Thickness(0, -30 + (60 + ((Thickness)value).Top) / 2, 0, ((Thickness)value).Top / 2);//manually replace this value : content grid margin and bar margin
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();//can't
    }
}
