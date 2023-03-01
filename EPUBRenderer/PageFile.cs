using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace EPUBRenderer
{
    internal class PageFile
    {
        public List<Letter> Content = new();
        public List<RenderPage> Pages = new List<RenderPage>();
        private int UsedCachePages = 0;
        private List<RenderPage> CachedPages = new List<RenderPage>();
        internal int Index;

        public PageFile(EpubPage page, CSSExtract CSS) => CreateContent(page, CSS);

        private RenderPage GetFreshPage()
        {
            if (CachedPages.Count <= UsedCachePages) CachedPages.Add(new RenderPage(this));
            var res = CachedPages[UsedCachePages++];
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
                var fitLetters = CurrentPage.Position(PageSize);
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
            WordInfo wordInfo = new();
            bool lastImage = false;
            foreach (var Part in page.Lines.SelectMany(a => a.Parts))
            {
                wordInfo.Style = GetStyle(Part, CSS);
                wordInfo.IsRuby = Part.IsRuby;
                switch (Part.Type)
                {
                    case LinePartTypes.marker:
                        var MarkerPart = (ChapterMarkerLinePart)Part;
                        Content.Add(new MarkerLetter(MarkerPart.Id, wordInfo));
                        break;
                    case LinePartTypes.normal:
                        lastImage = false;//removes trailing breaks after image
                        var TextPart = (TextLinePart)Part;
                        Letter prevLetter = Content.LastOrDefault();
                        char prevChar = 'a';//random character not in LineBreaks dicts
                        foreach (var Character in TextPart.Text)
                        {
                            bool NewWordBefore = TextPart.Splittable && CharInfo.PossibleLineBreaksBefore.Contains(Character);
                            bool NewWordAfter = TextPart.Splittable && CharInfo.PossibleLineBreaksAfter.Contains(Character);

                            var letter = new TextLetter(Character, wordInfo);
                            if (NewWordBefore && prevLetter != null) prevLetter.IsWordEnd = true;
                            if (NewWordAfter) letter.IsWordEnd = true;
                            Content.Add(prevLetter = letter);
                            prevChar = Character;
                        }
                        prevLetter.IsWordEnd = true;
                        break;
                    case LinePartTypes.image:
                        var ImagePart = (ImageLinePart)Part;
                        Content.Add(new ImageLetter(ImagePart.GetImage(), ImagePart.Inline, wordInfo));
                        lastImage = !ImagePart.Inline;
                        break;
                    case LinePartTypes.paragraph:
                        if (!lastImage) Content.Add(new BreakLetter(wordInfo));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            //get word references
            PageExtractDef extract = new();
            List<int> indexes = new();
            for (int i = 0; i < Content.Count; i++) if (Content[i].IsWordEnd) indexes.Add(i);
            List<Word> words = new(indexes.Count);

            foreach (var i in indexes)
            {
                extract.endLetter = i;
                words.Add(new Word(Content, extract));
                extract.startLetter = i + 1;
            }
            {//scope for prevLetter
                Letter prevLetter = null;
                for (int i = 0; i < words.Count; i++)
                {
                    Word own = words[i];
                    Word prev = i == 0 ? null : words[i - 1];
                    Word next = i == words.Count - 1 ? null : words[i + 1];
                    foreach (var letter in own.Letters)
                    {
                        letter.OwnWord = own;
                        letter.PrevWord = prev;
                        letter.NextWord = next;
                        letter.PrevLetter = prevLetter;
                        prevLetter = letter;
                    }
                }
            }
        }
    }
}

