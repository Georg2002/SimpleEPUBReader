﻿using EPUBParser;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EPUBRenderer
{
    public static class ChapterPagesCreator
    {
        public static double FontSize = 25;
        //all in times font size
        public static double LineSpace = 1.8;
        public static double RubyFontSize =0.5;
        public static double RubyOffSet = 1.1;

        public static List<PageRenderer> GetRenderPages(EpubPage Page, Vector PageSize)
        {           
            FlowDirectionModifiers.SetDirection(Page.PageSettings);

            PageRenderer NewPage;
            List<PageRenderer> Pages = new List<PageRenderer>();
            void GetNewPage()
            {               
                NewPage = new PageRenderer(Page.PageSettings, PageSize);
                NewPage.CurrentWritePos = FlowDirectionModifiers.GetStartWritingPosition(PageSize);               
            }
            GetNewPage();

            foreach (var Line in Page.Lines)
            {
                foreach (var Part in Line.Parts)
                {
                    if (Part.Type == LinePartTypes.image)
                    {
                        var ImagePart = (ImageLinePart)Part;
                        if (!NewPage.Fits(ImagePart) && (Line.Parts.Count > 1 || Page.Lines.Count >1 ))
                        {
                            Pages.Add(NewPage);
                            GetNewPage();
                        }
                        NewPage.AddImage(ImagePart);
                    }
                    else
                    {
                        var TextPart = (TextLinePart)Part;
                        if (TextPart.Type == LinePartTypes.sesame || !string.IsNullOrEmpty(TextPart.Ruby))
                        {
                            List<Writing> MainTextWritings = NewPage.GetMainTextWritings(TextPart.Text);
                            if (!NewPage.InPage(MainTextWritings.Last().WritingPosition))
                            {
                                Pages.Add(NewPage);
                                GetNewPage();
                                MainTextWritings = NewPage.GetMainTextWritings(TextPart.Text);
                            }

                            NewPage.Write(MainTextWritings);
                            NewPage.CurrentWritePos = MainTextWritings.Last().WritingPosition;
                            string Ruby = PageRenderer.GetRuby(TextPart);
                            List<Writing> RubyWritings = NewPage.GetRubyWritings(Ruby, MainTextWritings);
                            NewPage.Write(RubyWritings);
                        }
                        else
                        {
                            List<string> Words = new List<string>();                           
                            int SubstringIndex = 0;
                            int Length = 0;
                            for (int i = 0; i < TextPart.Text.Length; i++)
                            {
                                var c = TextPart.Text[i];       
                                Length++;
                                if (GlobalSettings.PossibleLineBreaks.Contains(c))
                                {
                                    if (i == TextPart.Text.Length - 1 || !GlobalSettings.PossibleLineBreaks.Contains(TextPart.Text[i+1]))
                                    {
                                        Words.Add(TextPart.Text.Substring(SubstringIndex, Length));
                                        Length = 0;
                                        SubstringIndex = i + 1;
                                    }                                    
                                }                                                            
                            }
                            Words.Add(TextPart.Text.Substring(SubstringIndex, Length));
                            foreach (var Word in Words)
                            {
                                if (Word.Length == 0)
                                {
                                    continue;
                                }
                                List<Writing> MainTextWritings = NewPage.GetMainTextWritings(Word);
                                if (!NewPage.InPage(MainTextWritings.Last().WritingPosition))
                                {
                                    Pages.Add(NewPage);
                                    GetNewPage();
                                    MainTextWritings = NewPage.GetMainTextWritings(Word);
                                }
                                NewPage.CurrentWritePos = MainTextWritings.Last().WritingPosition;
                                NewPage.Write(MainTextWritings);
                            }
                        }                        
                    }
                }
                NewPage.CurrentWritePos = FlowDirectionModifiers.NewLinePosition(NewPage.CurrentWritePos, PageSize);
            }
            Pages.Add(NewPage);
            return Pages;
        }
    }
}