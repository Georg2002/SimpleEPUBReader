using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows;

namespace EPUBRenderer3
{
    internal class PageFile
    {
        public List<Letter> Content;
        public List<RenderPage> Pages = new List<RenderPage>();
        private int UsedCachePages = 0;
        private List<RenderPage> CachedPages = new List<RenderPage>();
        internal int Index;

        public PageFile(EpubPage page, CSSExtract CSS) => CreateContent(page, CSS);

        private RenderPage GetFreshPage()
        {
            if (CachedPages.Count < UsedCachePages++) CachedPages.Add(new RenderPage(this));
            var res = CachedPages[UsedCachePages];
            return res;
        }
        public void CalculatePages(Vector PageSize, int Index)
        {
            this.Index = Index;
            Pages.Clear();
            UsedCachePages = 0;
            var CurrentPage = GetFreshPage();

            //fit using indexes without creating new objects
            void FitWords(PageExtractDef extract)
            {
                CurrentPage.Extract = extract;
                var fitLetters = CurrentPage.Position(null, PageSize);
                if (fitLetters < extract.length)
                {
                    var (fittingExtract, overflowExtract) = extract.Split(fitLetters);
                    CurrentPage.Extract.endLetter = fittingExtract.endLetter;
                    Pages.Add(CurrentPage);
                    CurrentPage = GetFreshPage();
                    FitWords(overflowExtract);
                }
                else
                {
                    CurrentPage.Extract.endLetter = extract.endLetter;
                }
            }

            FitWords(new PageExtractDef() { startLetter = 0, endLetter = Content.Count - 1 });
            Pages.Add(CurrentPage);
        }

        bool PosValid(PosDef Pos) => Content.Count > Pos.Letter && Pos.Letter >= 0;

        public override string ToString() => string.Join("", Content.Select(a => a.ToString()));

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

        private void CreateContent(EpubPage page, CSSExtract CSS)
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
                            this.Content.Add(new MarkerLetter(MarkerPart.Id, wordInfo));
                            break;
                        case LinePartTypes.normal:
                            var TextPart = (TextLinePart)Part;
                            Letter prevLetter = Content.LastOrDefault();
                            char prevChar = 'a';//random character not in LineBreaks dicts
                            foreach (var Character in TextPart.Text)
                            {
                                bool NewWordBefore = TextPart.Splittable && CharInfo.PossibleLineBreaksBefore.Contains(Character);
                                bool NewWordAfter = TextPart.Splittable && CharInfo.PossibleLineBreaksAfter.Contains(prevChar) && !CharInfo.PossibleLineBreaksAfter.Contains(Character);

                                if (NewWordBefore)
                                {
                                    if (prevLetter != null) prevLetter.IsWordEnd = true;
                                }
                                else
                                {
                                    if (NewWordAfter) prevLetter.IsWordEnd = true;//can't be null
                                    prevLetter = new TextLetter(Character, wordInfo);
                                    Content.Add(prevLetter);
                                }
                                prevChar = Character;
                            }
                            prevLetter.IsWordEnd = true;
                            break;
                        case LinePartTypes.image:
                            var ImagePart = (ImageLinePart)Part;
                            Content.Add(new ImageLetter(ImagePart.GetImage(), ImagePart.Inline));
                            break;
                        case LinePartTypes.paragraph:
                            Content.Add(new BreakLetter());
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
