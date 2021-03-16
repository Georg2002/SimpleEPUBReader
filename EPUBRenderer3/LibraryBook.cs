﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer3
{
    public struct LibraryBook
    {
        public string FilePath;
        public PosDef CurrPos;
        public List<MarkingDef> Markings;
    }
}