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
            if (CurrBook != null)
            {
                OpenPage(CurrBook.CurrPos);
            }
        }

        public void LoadBook(string Path, PosDef Position = new PosDef(), List<MarkingDef> Markings = null)
        {
            Markings = Markings == null ? new List<MarkingDef>() : Markings;
            Epub epub = new Epub(Path);
            CurrBook = new RenderBook(epub);
            OpenPage(Position);
        }


        public void OpenPage(PosDef Position)
        {
            CurrBook.CurrPos = Position;

            CurrBook.Position(PageSize);
            var PageFile = CurrBook.PageFiles[Position.FileIndex];
            ShownPage = PageFile.Pages.Find(a => a.Within(Position));
            InvalidateVisual();
        }

        public void Switch(int Dir)
        {
            int FileIndex = CurrBook.CurrPos.FileIndex;
            int PageIndex = CurrBook.PageFiles[FileIndex].Pages.IndexOf(ShownPage);
            PageIndex += Dir;
            if (PageIndex < 0)
            {                
                if (FileIndex != 0)
                {
                    FileIndex--;
                    PageIndex = CurrBook.PageFiles[FileIndex].Pages.Count - 1;
                }
                else
                {
                    PageIndex++;
                }
            }
            else if (PageIndex >= CurrBook.PageFiles[FileIndex].Pages.Count)
            {                
                if (FileIndex != CurrBook.PageFiles.Count - 1)
                {
                    FileIndex++;
                    PageIndex = 0;
                }
                else
                {
                    PageIndex--;
                }
            }

            OpenPage(CurrBook.PageFiles[FileIndex].Pages[PageIndex].StartPos);
        }
    }
}
