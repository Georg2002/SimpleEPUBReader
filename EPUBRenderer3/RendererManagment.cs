using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EPUBParser;

namespace EPUBRenderer3
{
    public partial class Renderer : FrameworkElement
    {
        RenderBook CurrBook;

        public void LoadBook(string Path, PosDef Position = new PosDef(), List<MarkingDef> Markings = null)
        {
            Markings = Markings == null ? new List<MarkingDef>() : Markings;
            Epub epub = new Epub(Path);
            CurrBook = new RenderBook(epub);

        }
    }
}
