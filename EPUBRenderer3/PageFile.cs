using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows;

namespace EPUBRenderer3
{
    internal struct PageExtractDef
    {
        public int startWord;//index of first partial word
        public int startLetter;
        public int endWord; //index of last complete word
        public int endLetter;

        internal Tuple<PageExtractDef, PageExtractDef> SplitWords(List<Word> words, PageExtractDef extract, int WordCount, int LetterCount)
        {
            //Word count is words including partial, letter count are the letters of the last (maybe partial) word
            PageExtractDef front = new PageExtractDef();
            PageExtractDef rear = new PageExtractDef();
            //at first only the full words are added, then the partial word is split and added
            front.startLetter = extract.startLetter;
            front.startWord = extract.startWord;
            front.endWord = front.startWord + WordCount - 1;
            front.endLetter = extract.endLetter;

            int relLetterCount = words[extract.endWord].Letters.Count;
            //partial last word
            if (relLetterCount < LetterCount)
            {
                rear.startLetter = front.endLetter + 1;
                rear.startWord = front.endWord;
            }
            else
            {
                rear.startLetter = 0;
                rear.startWord = front.endWord + 1;
            }
            rear.endLetter = extract.endLetter;
            rear.endWord = extract.endWord;
            return new Tuple<PageExtractDef, PageExtractDef>(front, rear);
        }
    }
    internal class PageFile
    {
        public List<Letter> Letters;
        public List<RenderPage> Pages = new List<RenderPage>();
        private int UsedCachePages = 0;
        private List<RenderPage> CachedPages = new List<RenderPage>();
        internal int Index;

        public PageFile(EpubPage page, CSSExtract CSS) => PreparePage(page, CSS);

        private RenderPage GetFreshPage()
        {
            if (CachedPages.Count < UsedCachePages++) CachedPages.Add(new RenderPage());
            var res = CachedPages[UsedCachePages];
            return res;
        }
        public void PositionText(Vector PageSize, int Index)
        {
            this.Index = Index;
            Word Prev = null;
            Pages.Clear();
            UsedCachePages = 0;
            var CurrentPage = GetFreshPage();

            //fit using indexes without creating new objects
            void FitWords(List<Word> words, PageExtractDef extract)
            {
                var (FitWord, FitLetter) = Word.PositionWords(words, PageSize);
                if (FitWord < words.Count)
                {
                    var (fittingWords, overflowWords) = PageExtractDef.SplitWords(words, FitWord, FitLetter);
                    if (FitWord > 0) CurrentPage.Words.AddRange(fittingWords);
                    Pages.Add(CurrentPage);
                    Prev = null;
                    CurrentPage = GetFreshPage();
                    FitWords(overflowWords);
                }
                else
                {
                    CurrentPage.Words.AddRange(words);
                    Prev = CurrentPage.Words.Last();
                }
            }

            FitWords(Words, 0, 0, Words.Count, Words.Last().Letters.Count());
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
                    NewStyle.Width = Style.Width * TextLetter.StandardFontSize;
                    NewStyle.Height = Style.Height * TextLetter.StandardFontSize;
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

        private List<Letter> PreparePage(EpubPage page, CSSExtract CSS)
        {
            WordInfo wordInfo = new WordInfo();

            foreach (var RawLine in page.Lines)
            {
                foreach (var Part in RawLine.Parts)
                {
                    wordInfo.Style = GetStyle(Part, CSS);
                    wordInfo.IsRuby = Part.IsRuby;
                    switch (Part.Type)
                    {
                        case LinePartTypes.marker:
                            var MarkerPart = (ChapterMarkerLinePart)Part;
                            this.Letters.Add(new MarkerLetter(MarkerPart.Id, wordInfo));
                            break;
                        case LinePartTypes.normal:
                            var TextPart = (TextLinePart)Part;

                            char Prev = 'a';
                            foreach (var Character in TextPart.Text)
                            {
                                bool NewWordBefore = TextPart.Splittable && CharInfo.PossibleLineBreaksBefore.Contains(Character);
                                bool NewWordAfter = TextPart.Splittable && CharInfo.PossibleLineBreaksAfter.Contains(Prev) && !CharInfo.PossibleLineBreaksAfter.Contains(Character);

                                if (NewWordBefore)
                                {
                                    var prev = Letters.LastOrDefault();
                                    if (prev != null) prev.IsWordEnd = true;
                                }
                                else
                                {
                                    if (NewWordAfter) this.Letters.LastOrDefault().IsWordEnd = true;
                                    this.Letters.Add(new TextLetter(Character, wordInfo));
                                }

                                Prev = Character;
                            }
                            if (Word.Letters.Count != 0) AddWordToList(Part);

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
