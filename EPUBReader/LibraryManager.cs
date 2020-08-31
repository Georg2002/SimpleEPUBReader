using EPUBParser;
using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader
{
    public static class LibraryManager
    {
        public static LibraryDisplayer LibDisp;
        public static bool InUse;
        internal static BookDefinition SelectedItem;
        public static List<BookDefinition> Books = new List<BookDefinition>();

        internal static void LoadSelectedBook()
        {
            if (SelectedItem != null)
            {
                ViewerInteracter.LoadBookDefinition(SelectedItem);
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
            foreach (var Book in Books)
            {
                if (!File.Exists(Book.FilePath))
                {
                    Books.Remove(Book);
                }
            }
        }
    }
}
