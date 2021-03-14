using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer3
{
    internal class PageFile
    {
        public List<Line> Lines;
        public List<RenderPage> Pages;

        public PageFile(EpubPage page)
        {
            Lines = SplitAndOrder(page);
        }

        public void PositionText(Vector PageSize)
        {
            Line Prev = null;
            Pages = new List<RenderPage>();
            var CurrentPage = new RenderPage();

            void FitLine(Line Line)
            {
                var (FitWord ,FitLetter) = Line.Position(Prev, PageSize);
                if (FitWord < Line.Words.Count)
                {
                    var (FittingLine, OverflowLine) = Line.Split(FitWord, FitLetter);                 
                    CurrentPage.Lines.Add(FittingLine);
                    Pages.Add(CurrentPage);
                    CurrentPage = new RenderPage();
                    FitLine(OverflowLine);
                }
                else
                {
                    CurrentPage.Lines.Add(Line);
                }
            }

            foreach (var Line in Lines)
            {
                FitLine(Line);
            }

            var Curr = new PosDef(-1, 0, 0, 0);

            for (int i = 0; i < Pages.Count; i++)
            {
                var CurrPage = Pages[i];
                CurrPage.StartPos = Curr;

                if (CurrPage.Lines.Count == 1)
                {
                    if (CurrPage.Lines[0].Words.Count == 1)
                    {
                        if (CurrPage.Lines[0].Words[0].Letters.Count == 1)
                        {

                        }
                        else
                        {
                            Curr.Letter += CurrPage.Lines[0].Words[0].Letters.Count - 1;
                        }
                    }
                    else
                    {
                        Curr.Word += CurrPage.Lines[0].Words.Count - 1;
                        Curr.Letter = CurrPage.Lines[0].Words.Last().Letters.Count - 1;
                    }
                }
                else
                {
                    Curr.Line += CurrPage.Lines.Count - 1;
                    Curr.Word = CurrPage.Lines.Last().Words.Count - 1;
                    Curr.Letter = CurrPage.Lines.Last().Words.Last().Letters.Count - 1;                    
                }
            }
        }



        public override string ToString()
        {
            string Text = "";
            Lines.ForEach(a => Text = Text + a);
            return Text;
        }



        private List<Line> SplitAndOrder(EpubPage page)
        {
            var Lines = new List<Line>();

            foreach (var RawLine in page.Lines)
            {
                var Line = new Line();
                var Word = new Word();

                foreach (var Part in RawLine.Parts)
                {
                    switch (Part.Type)
                    {
                        case LinePartTypes.sesame:
                        case LinePartTypes.normal:
                            var TextPart = (TextLinePart)Part;
                            bool NoRuby = string.IsNullOrEmpty(TextPart.Ruby) && TextPart.Type != LinePartTypes.sesame;
                            char Prev = 'a';
                            foreach (var Character in TextPart.Text)
                            {
                                if (NoRuby && CharInfo.PossibleLineBreaks.Contains(Prev) && !CharInfo.PossibleLineBreaks.Contains(Character))
                                {
                                    Line.Words.Add(Word);
                                    Word = new Word();
                                }
                                Word.Letters.Add(new TextLetter(Character));
                                Prev = Character;
                            }
                            Line.Words.Add(Word);
                            Word = new Word();
                            if (!NoRuby)
                            {
                                if (!string.IsNullOrEmpty(TextPart.Ruby))
                                {
                                    foreach (var Character in TextPart.Ruby)
                                    {
                                        Word.Letters.Add(new TextLetter(Character));
                                    }
                                    Word.Type = WordTypes.Ruby;
                                }
                                else if (TextPart.Type == LinePartTypes.sesame)
                                {
                                    int Length = Line.Words.Last().Letters.Count;
                                    for (int i = 0; i < Length; i++)
                                    {
                                        Word.Letters.Add(new TextLetter('﹅'));
                                    }
                                    Word.Type = WordTypes.Ruby;
                                }
                                Line.Words.Add(Word);
                                Word = new Word();
                            }
                            break;
                        case LinePartTypes.image:
                            var ImagePart = (ImageLinePart)Part;
                            Word.Letters.Add(new ImageLetter(ImagePart.GetImage()));
                            Word.Letters.Add(new BreakLetter());
                            Line.Words.Add(Word);
                            Word = new Word();
                            break;
                        case LinePartTypes.paragraph:
                            Word.Letters.Add(new BreakLetter());
                            Line.Words.Add(Word);
                            Word = new Word();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                Lines.Add(Line);
            }

            return Lines;
        }
    }
}
