using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace EPUBRenderer
{
    public static class ChapterPagesCreator
    {
        public static double FontSize = 25;
        //all in times font size
        public static double LineSpace = 2;
        public static double RubyFontSize = 0.5;
        public static double RubyOffSet = 1.5;

        public static List<PageRenderer> GetRenderPages(EpubPage Page, double Width, double Height)
        {
            FlowDirectionModifiers.SetDirection(Page.PageSettings);

            PageRenderer NewPage = new PageRenderer(Page.PageSettings, Width, Height);            
            List<PageRenderer> Pages = new List<PageRenderer>();

            foreach (var Line in Page.Lines)
            {
                foreach (var Part in Line.Parts)
                {
                    if (Part.Type == LinePartTypes.image)
                    {
                        if (!NewPage.Fits((ImageLinePart)Part))
                        {
                            Pages.Add(NewPage);
                            NewPage = new PageRenderer(Page.PageSettings, Width, Height);
                        }
                        NewPage.AddImage(Part);
                    }
                    else
                    {
                        var TextPart = (TextLinePart)Part;
                        if (TextPart.Type == LinePartTypes.sesame || !string.IsNullOrEmpty(TextPart.Ruby))
                        {
                            NewPage.Write(TextPart);
                        }
                        else
                        {
                            var Words = TextPart.Text.Split(GlobalSettings.PossibleLineBreaks);
                            foreach (var Word in Words)
                            {
                                List<Writing> writings = NewPage.GetMainTextWritings(Word);
                                if (!NewPage.InPage(writings.Last().WritingPosition))
                                {
                                    Pages.Add(NewPage);
                                    NewPage = new PageRenderer(Page.PageSettings, Width, Height);
                                }
                                NewPage.Write(Word);
                            }
                        }
                    }
                }
            }

            return Pages;
        }
    }
}
