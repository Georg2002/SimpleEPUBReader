using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader
{
    public struct SaveObject
    {
        public string LastOpen;
        public int LastRenderPageIndex;
        public double RenderPageRatio;
        public bool Nightmode;
        public List<string> LibraryPaths;
        public List<MarkingDefinition> Markings;
    }
}
