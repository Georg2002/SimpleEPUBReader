using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
