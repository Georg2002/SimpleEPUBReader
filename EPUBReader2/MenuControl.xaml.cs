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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EPUBRenderer3;

namespace EPUBReader2
{
    /// <summary>
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class MenuControl : UserControl
    {
     public   bool ShowingChapters = false;

        public MenuControl()
        {
            InitializeComponent();
        }

        public MainWindow Main;

        public void SetToChapters(List<string> Chapters)
        {
            ShowingChapters = true; var Items = new List<ListItemStruct>();
            for (int i = 0; i < Chapters.Count; i++)
            {
                var Chapter = Chapters[i];
                Items.Add(new ListItemStruct(Chapter, i, false, DateTime.Now));
            }
            ListBox.ItemsSource = Items;
        }

        public void SetToLibrary(List<LibraryBook> Books)
        {
            ShowingChapters = false;
            var Items = new List<ListItemStruct>();
            for (int i = 0; i < Books.Count; i++)
            {
                var Book = Books[i];
                Items.Add(new ListItemStruct(Book.Title, i, true, Book.DateAdded));
            }
            ListBox.ItemsSource = Items;
        }     

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var Button = (Button)sender;
            var Item = (ListItemStruct)Button.DataContext;
            Main.DeleteBook(Item.Index);
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var Button = (Button)sender;
            var Item = (ListItemStruct)Button.DataContext;
            if (ShowingChapters)
            {
                Main.SetChapter(Item.Index);
            }
            else
            {
                Main.SetToBook(Item.Index);
            }
        }
    }

    struct ListItemStruct
    {
        public string Text { get; set; }
        public string Date { get; set; }
        public int Number { get; set; }
        public int Index;
        public Visibility visibility { get; set; }
        public ListItemStruct(string Text, int Index, bool IsBook,DateTime Date)
        {
            this.Text = Text;
            Number = Index + 1;
            this.Index = Index;
            visibility = IsBook ? Visibility.Visible : Visibility.Collapsed;
            this.Date = Date.ToString("dd.MM.yyyy");
        }
    }
}
