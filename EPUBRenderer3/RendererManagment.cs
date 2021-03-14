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

        Vector PageSize;

        RenderPage ShownPage = null;

        public Renderer()
        {
            SizeChanged += Renderer_SizeChanged;
        }

        private void Renderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageSize = new Vector(ActualWidth, ActualHeight);
        }

        public void LoadBook(string Path, PosDef Position = new PosDef(), List<MarkingDef> Markings = null)
        {
            Markings = Markings == null ? new List<MarkingDef>() : Markings;
            Epub epub = new Epub(Path);
            CurrBook = new RenderBook(epub);
          //  OpenPage
        }


        public void OpenPage(PosDef Position)
        {
          var  PageFile = CurrBook.PageFiles[Position.FileIndex];
           // int PageIndex = Position. PageFile.Pages.Count
        }
    }
}
