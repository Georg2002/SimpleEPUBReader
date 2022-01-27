using EPUBRenderer3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader2
{
    public class Library
    {
        private List<LibraryBook> Books = new List<LibraryBook>();

        public void AddOrReplaceBook(LibraryBook NewBook)
        {
            if (string.IsNullOrEmpty(NewBook.FilePath)) return;
            for (int i = 0; i < Books.Count; i++)
            {
                var Book = Books[i];
                if (Book.FilePath == NewBook.FilePath)
                {
                    Books[i] = NewBook;
                    return;
                }                
            }
            Books.Insert(0,NewBook);
        }
            
        public void DeleteBook(int Index)
        {
            Books.RemoveAt(Index);
        }

        internal LibraryBook GetBook(int index)
        {
            if (index <0 || index >= Books.Count) return new LibraryBook();          
            return Books[index];
        }

        internal void SetFromSave(List<LibraryBook> books)
        {
            this.Books = books;
        }

        internal int GetIndex(LibraryBook book)
        {
            return Books.IndexOf(book);
        }

        internal List<LibraryBook> GetBooks()
        {
            return Books;
        }
    }
}
