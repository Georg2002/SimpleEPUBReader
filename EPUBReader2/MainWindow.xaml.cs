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
using System.Windows.Navigation;
using EPUBRenderer3;
using Microsoft.Win32;

namespace EPUBReader2
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
        private const byte Alpha = 100;
        readonly Brush[] MarkingColors = new Brush[] {null,
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
            Bar.Margin = new Thickness(0, -MouseManager.BarHeight, 0, 0);
            ContentGrid.Margin = new Thickness(0, MouseManager.BarHeight / 2, 0, MouseManager.BarHeight / 2);
            PagesControl.Main = this;
            Menu.Main = this;            
        }



        private void LoadSave()
        {
            SaveStruc Save = SaveAndLoad.LoadSave();
            ColorIndex = Save.ColorIndex != 0 && Save.ColorIndex < MarkingColors.Length ? Save.ColorIndex : (byte)1;
            ColorButton.Background = MarkingColors[ColorIndex];
            if (!Directory.Exists(Path.GetPathRoot(Save.LastDirectory)))
            {
                Save.LastDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            while (!Directory.Exists(Save.LastDirectory))
            {
                Save.LastDirectory = Path.Combine(Save.LastDirectory, @"\..");
            }
            Dialog.InitialDirectory = Save.LastDirectory;
            if (Save.Books != null)
            {
                Library.SetFromSave(Save.Books);
                if (Save.CurrentBookIndex != 0 && Save.CurrentBookIndex < Save.Books.Count)
                {
                    SetToBook(Save.CurrentBookIndex);
                }
            }
            if (Save.Fullscreen) Fullscreen_Click(null, null);
        }

        internal void DeleteBook(int index)
        {
            Library.DeleteBook(index);
            Menu.SetToLibrary(Library.GetTitles());
        }

        internal int GetCurrentPage()
        {
            return Renderer.GetCurrentPage();
        }

        internal void SetToBook(int LibraryIndex)
        {
            LibraryBook Book = Library.GetBook(LibraryIndex);
            Renderer.LoadBook(Book.FilePath, Book.CurrPos, Book.Markings);
        }

        internal int GetPageCount()
        {
            return Renderer.GetPageCount();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //C:\Users\georg\Desktop\b\Zeug\a\Learning\Books\日常\[ìýüEèG_éáéþé¯ é»éóéóé┐ & Æÿ_ê╔ôñ ò¢É¼] ô·ÅÝé╠ë─ïxé¦.epub
            //D:\Informatik\EPUBReader\TestResources\DanMachi.epub
            //D:\Informatik\EPUBReader\TestResources\星界の紋章第一巻.epub

            var Args = Environment.GetCommandLineArgs();
            if (Args.Length > 1 && File.Exists(Args[1]) && Args[1].ToLower().EndsWith(".epub"))
            {
                Renderer.LoadBook(Args[1]);
            }
            LoadSave();
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Renderer.Switch(-1);
            PagesControl.Refresh();
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Renderer.Switch(1);
            PagesControl.Refresh();
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
                Renderer.LoadBook(Dialog.FileName);
                Dialog.InitialDirectory = Path.GetDirectoryName(Dialog.FileName);
            }
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Visibility == Visibility.Visible)
            {

                Menu.Visibility = Visibility.Collapsed;
                MouseManager.Locked = false;
                FunctionsLocked = false;
            }
            else
            {
                if (FunctionsLocked) return;
                MouseManager.Locked = true;
                FunctionsLocked = true;
                Menu.Visibility = Visibility.Visible;
                Library.AddOrReplaceBook(Renderer.GetCurrentBook());
                Menu.SetToLibrary(Library.GetTitles());
            }

        }

        private void Chapter_Click(object sender, RoutedEventArgs e)
        {
            if (Menu.Visibility == Visibility.Visible)
            {

                Menu.Visibility = Visibility.Collapsed;
                MouseManager.Locked = false;
                FunctionsLocked = false;
            }
            else
            {
                if (FunctionsLocked) return;
                MouseManager.Locked = true;
                FunctionsLocked = true;
                Menu.Visibility = Visibility.Visible;
                Menu.SetToChapters(Renderer.GetChapters());
            }
        }

        public void SetChapter(int ChapterIndex)
        {
            Renderer.SetChapter(ChapterIndex);
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
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            ColorIndex++;
            if (ColorIndex == MarkingColors.Length)
            {
                ColorIndex = 1;
            }
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

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();           
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
            return Save;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PagesControl.Refresh();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveAndLoad.Save(GetSave());
        }             
    }
}
