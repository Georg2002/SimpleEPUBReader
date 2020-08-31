using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader
{
    public class SaveObject
    {       
        public BookDefinition LastBook;
        public bool Nightmode;
        public List<BookDefinition> LibraryBooks;       
    }

    public class BookDefinition
    {
        public List<MarkingDefinition> Markings;
        public int LastRenderPageIndex;
        public double RenderPageRatio;
        public string FilePath;
        public string Title { get; set; }
    }
}
