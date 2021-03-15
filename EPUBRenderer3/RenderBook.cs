using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer3
{
    internal class RenderBook
    {
        private Epub epub;
        public List<PageFile> PageFiles;
        public PosDef CurrPos;

        public RenderBook(Epub epub)
        {
            this.epub = epub;
            PageFiles = new List<PageFile>();
            foreach (var Page in epub.Pages)
            {
                PageFiles.Add(new PageFile(Page));
            }
        }

        internal void Position(Vector pageSize)
        {
        //    for (int i = 0; i < PageFiles.Count; i++)
        //    {
        //        PageFiles[i].PositionText(pageSize, i);
        //    }
          Parallel.For(0, PageFiles.Count, a => PageFiles[a].PositionText(pageSize, a));
        }
    }
}
