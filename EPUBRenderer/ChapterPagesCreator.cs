using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public static class ChapterPagesCreator
    {
        public static double FontSize = 25;
        //all in times font size
        public static double LineSpace = 2;
        public static double RubyFontSize = 0.5;
        public static double RubyOffSet = 1.5;

        public static List<PageRenderer2> GetRenderPages(EpubPage Page, double Width, double Height)
        {
            PageRenderer2 NewPage = new PageRenderer2(Page.PageSettings);

            List<PageRenderer2> Pages = new List<PageRenderer2>();

            foreach (var Line in Page.Lines)
            {
                foreach (var Part in Line.Parts)
                {
                    if (Part.Type == LinePartTypes.image)
                    {
                        if (!NewPage.Fits(Part))
                        {
                            Pages.Add(NewPage);
                            NewPage = new PageRenderer2(Page.PageSettings);
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
                                if (!NewPage.Fits(Word))
                                {
                                    Pages.Add(NewPage);
                                    NewPage = new PageRenderer2(Page.PageSettings);
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
