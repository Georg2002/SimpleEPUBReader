using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
    internal class RenderBook
    {
        private Epub epub;
        public List<PageFile> PageFiles;

        public RenderBook(Epub epub)
        {
            this.epub = epub;
            PageFiles = new List<PageFile>();
            foreach (var Page in epub.Pages)
            {
                PageFiles.Add(new PageFile(Page));
            }

        }
    }
}
