using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
    public struct LibraryBook
    {
        public string FilePath;
        public string Title;
        public PosDef CurrPos;
        public List<MrkDef> Markings;
        public DateTime DateAdded;
    }
}
