using EPUBParser;
using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBReader
{
    public static class LibraryManager
    {
        public static ListSelector Selector;
        public static List<BookDefinition> Books = new List<BookDefinition>();

        internal static void LoadSelectedBook(object sender, EventArgs e)
        {
            int i = Selector.SelectedIndex;
            if (i >= 0 && i < Books.Count)
            {
                ViewerInteracter.LoadBookDefinition(Books[i]);
            }          
        }

        internal static void TryAddBook(Epub Epub)
        {
            BookDefinition NewBook = new BookDefinition
            {
                Title = Epub.Package.Title,
                FilePath = Epub.FilePath,
                Markings = new List<MarkingDefinition>(),
                LastRenderPageIndex = 1,
                RenderPageRatio = 0
            };
            bool Add = true;
            foreach (var Book in Books)
            {
                if (Book.FilePath == NewBook.FilePath)
                {
                    Add = false;
                    break;
                }
            }
            if (Add)
            {
                Books.Add(NewBook);
            }
        }

        private static List<string> GetTitleList()
        {
            List<string> Titles = new List<string>();
            foreach (var Book in Books)
            {
                Titles.Add(Book.Title);
            }
            return Titles;
        }

        internal static void UpdateCurrentBook()
        {
            string FilePath = ViewerInteracter.GetCurrentPath();

            foreach (var Book in Books)
            {
                if (Book.FilePath == FilePath)
                {
                    Book.Markings = ViewerInteracter.GetAllMarkings();
                    Book.LastRenderPageIndex = ViewerInteracter.GetCurrentRenderPage();
                    Book.RenderPageRatio = ViewerInteracter.GetCurrentRenderPageRatio();
                    break;
                }
            }
        }

        internal static void CheckBookList()
        {          
            for (int i = 0; i < Books.Count; i++)
            {
                var Book = Books[i];
                if (!File.Exists(Book.FilePath))
                {
                    Books.Remove(Book);
                }
            }          
        }

        internal static void SetSelector()
        {
            UpdateCurrentBook();
            Selector.DeleteMenu.Visibility = Visibility.Visible;
            Selector.ItemSelected += LoadSelectedBook;
            Selector.ItemDeleted += DeleteBook;
            var Titles = GetTitleList();
            Selector.ShownList = Titles;
        }

        internal static void DeleteBook(object sender, EventArgs e)
        {
            int i = Selector.SelectedIndex;
            if (i >= 0 && i < Books.Count)
            {
                Books.RemoveAt(i);
                Selector.List.ItemsSource = GetTitleList();
                Selector.List.Items.Refresh();
            }
        }

        public static void ResetSelector()
        {
            Selector.DeleteMenu.Visibility = Visibility.Hidden;
            Selector.ItemSelected -= LoadSelectedBook;
            Selector.ItemDeleted -= DeleteBook;
        }
    }
}
