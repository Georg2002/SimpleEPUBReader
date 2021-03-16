﻿using EPUBRenderer3;
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
            Books.Add(NewBook);
        }

        public List<string> GetTitles()
        {
            var Res = new List<string>();
            foreach (var Book in Books)
            {
                Res.Add(Book.Title);
            }
            return Res;
        }

        public void DeleteBook(int Index)
        {
            Books.RemoveAt(Index);
        }

        internal LibraryBook GetBook(int index)
        {
            return Books[index];
        }
    }
}
