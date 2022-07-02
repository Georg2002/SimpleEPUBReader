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
        public List<Word> Words;
        public List<RenderPage> Pages;

        public PageFile(EpubPage page, CSSExtract CSS) => Words = PreparePage(page, CSS);

        public void PositionText(Vector PageSize, int Index)
        {
            Word Prev = null;
            Pages = new List<RenderPage>();
            var CurrentPage = new RenderPage();

            void FitWords(List<Word> words)
            {
                var (FitWord, FitLetter) = Word.PositionWords(words, PageSize);
                if (FitWord < words.Count)
                {
                    var (fittingWords, overflowWords) = Word.SplitWords(words, FitWord, FitLetter);
                    if (fittingWords.Count != 0) CurrentPage.Words.AddRange(fittingWords);
                    Pages.Add(CurrentPage);
                    Prev = null;
                    CurrentPage = new RenderPage();
                    FitWords(overflowWords);
                }
                else
                {
                    CurrentPage.Words.AddRange(words);
                    Prev = CurrentPage.Words.Last();
                }
            }

            FitWords(Words);
            Pages.Add(CurrentPage);

            var Curr = new PosDef(Index, 0, 0);

            for (int i = 0; i < Pages.Count; i++)
            {
                var CurrPage = Pages[i];
                CurrPage.StartPos = Curr;

                if (CurrPage.Words.Count == 1) Curr.Letter += CurrPage.Words[0].Letters.Count - 1;
                else Curr.Letter = CurrPage.Words.Last().Letters.Count - 1;

                Curr.Word += CurrPage.Words.Count - 1;

                CurrPage.EndPos = Curr;
                Curr.Increment(Words);
            }
        }

        bool PosValid(PosDef Pos) => Words.Count > Pos.Word && Words[Pos.Word].Letters.Count > Pos.Letter;

        public override string ToString() => string.Join("", Words.Select(a => a.ToString()));       

        private WordStyle GetStyle(BaseLinePart Part, CSSExtract CSS)
        {
            var NewStyle = new WordStyle();
            if (Part.ActiveClasses == null) return NewStyle;
            foreach (string SelectorText in Part.ActiveClasses)
            {
                var Style = CSS.Styles.FirstOrDefault(a => a.SelectorText == SelectorText);
                if (Style != null)
                {

                    NewStyle.RelativeFontSize = Style.FontSize;
                    switch (Style.FontWeight)
                    {//main font only supports up to W6, aka semi bold, or maybe none at all
                        case EPUBParser.FontWeights.bold:
                            NewStyle.Weight = System.Windows.FontWeights.SemiBold;
                            break;
                        case EPUBParser.FontWeights.bolder:
                            NewStyle.Weight = System.Windows.FontWeights.Medium;
                            break;
                        case EPUBParser.FontWeights.lighter:
                            NewStyle.Weight = System.Windows.FontWeights.Light;
                            break;
                        case EPUBParser.FontWeights.normal:
                            NewStyle.Weight = System.Windows.FontWeights.Normal;
                            break;
                    }
                }
            }
            return NewStyle;
        }

        private List<Word> PreparePage(EpubPage page, CSSExtract CSS)
        {
            var res = new List<Word>();

            foreach (var RawLine in page.Lines)
            {
                var Word = new Word();
                Word.Style = GetStyle(RawLine.Parts.FirstOrDefault(), CSS);

                void AddWordToList(BaseLinePart Part)
                {
                    res.Add(Word);
                    Word = new Word();
                    Word.Style = GetStyle(Part, CSS);
                }

                foreach (var Part in RawLine.Parts)
                {
                    Word.Style = GetStyle(Part, CSS);
                    switch (Part.Type)
                    {
                        case LinePartTypes.marker:
                            var MarkerPart = (ChapterMarkerLinePart)Part;
                            Word.Letters.Add(new MarkerLetter(MarkerPart.Id));
                            AddWordToList(Part);
                            break;
                        case LinePartTypes.sesame:
                        case LinePartTypes.normal:
                            var TextPart = (TextLinePart)Part;
                            bool NoRuby = string.IsNullOrEmpty(TextPart.Ruby) && TextPart.Type != LinePartTypes.sesame;
                            char Prev = 'a';
                            foreach (var Character in TextPart.Text)
                            {
                                bool NewWordBefore = NoRuby && CharInfo.PossibleLineBreaksBefore.Contains(Character);
                                bool NewWordAfter = NoRuby && CharInfo.PossibleLineBreaksAfter.Contains(Prev) && !CharInfo.PossibleLineBreaksAfter.Contains(Character);

                                if (NewWordBefore)
                                {
                                    if (Word.Letters.Count != 0) AddWordToList(Part);
                                    Word.Letters.Add(new TextLetter(Character, Word.Style));
                                }
                                else
                                {
                                    if (NewWordAfter) AddWordToList(Part);
                                    Word.Letters.Add(new TextLetter(Character, Word.Style));
                                }

                                Prev = Character;
                            }
                            if (Word.Letters.Count == 0) break;
                            AddWordToList(Part);

                            if (!NoRuby)
                            {
                                if (!string.IsNullOrEmpty(TextPart.Ruby))
                                {
                                    foreach (var Character in TextPart.Ruby) Word.Letters.Add(new TextLetter(Character, Word.Style));
                                    Word.Type = WordTypes.Ruby;
                                }
                                else if (TextPart.Type == LinePartTypes.sesame)
                                {
                                    int Length = res.Last().Letters.Count;
                                    for (int i = 0; i < Length; i++) Word.Letters.Add(new TextLetter('﹅', Word.Style));
                                }
                                AddWordToList(Part);
                            }
                            break;
                        case LinePartTypes.image:
                            var ImagePart = (ImageLinePart)Part;
                            Word.Letters.Add(new ImageLetter(ImagePart.GetImage(), ImagePart.Inline));
                            AddWordToList(Part);
                            break;
                        case LinePartTypes.paragraph:
                            Word.Letters.Add(new BreakLetter());
                            AddWordToList(Part);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return res;
        }
    }
}
