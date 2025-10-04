using EPUBParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EPUBRenderer
{
    internal class PageFile
    {
        public List<Letter> Content = new();
        private List<RenderPage> Pages = new();
        private int UsedCachePages = 0;
        private readonly List<RenderPage> CachedPages = new();
        internal int Index;
        internal static Typography.OpenFont.Typeface LookupTf;
        internal Task PositioningTask { get; private set; }
        static PageFile()
        {
            var reader = new Typography.OpenFont.OpenFontReader();
            // var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream()
            // var stream = File.Open(@"D:\Informatik\EPUBReader\EPUBRenderer\Fonts\NotoSansJP-Black.ttf", FileMode.Open, FileAccess.Read, FileShare.Read);
            var stream = Application.GetResourceStream(new Uri("pack://application:,,,/EPUBRenderer;component/Fonts/NotoSansJP-Regular.ttf")).Stream;
            var tf = reader.Read(stream);
            stream.Close();
            PageFile.LookupTf = tf;
        }
        private EpubPage epubPage;
        private CSSExtract CSS;
        private List<MrkDef> mrkDefs;
        public PageFile(EpubPage page, CSSExtract CSS, List<MrkDef> mrkDefs)
        {
            this.epubPage = page;
            this.CSS = CSS;
            this.mrkDefs = mrkDefs;
        }

        internal void Setup()
        {
            if (this.epubPage is null) return;
            this.CreateContent(this.epubPage, CSS);
            foreach (var mrk in mrkDefs.Where(a => a.Pos.Letter < this.Content.Count))
            {
                this.Content[mrk.Pos.Letter].MarkingColorIndex = mrk.ColorIndex;
            }
            this.epubPage.FreeMemory();
            this.epubPage = null;
            this.CSS = null;
            this.mrkDefs = null;
        }

        private RenderPage GetFreshPage()
        {
            if (CachedPages.Count <= UsedCachePages) CachedPages.Add(new RenderPage(this));
            var res = CachedPages[UsedCachePages++];
            return res;
        }

        private Vector lastPageSize = new(-1, -1);
        private readonly object lockO = new();
        public Task CalculatePages(Vector PageSize, int Index)
        {
            lock (this.lockO)
            {
                if (this.PositioningTask is not null && this.lastPageSize == PageSize) return this.PositioningTask;
                this.lastPageSize = PageSize;

                return this.PositioningTask = Task.Run(() =>
                      {

                          this.Setup();
                          this.Index = Index;
                          lock (this.Pages) Pages.Clear();
                          UsedCachePages = 0;
                          var CurrentPage = this.GetFreshPage();

                          //fit using indexes without creating new objects
                          void FitWords(PageExtractDef extract)
                          {
                              CurrentPage.Extract = extract;
                              var fitLetters = CurrentPage.Position(PageSize);
                              if (fitLetters < extract.Length)
                              {
                                  var (fittingExtract, overflowExtract) = extract.Split(fitLetters);
                                  CurrentPage.Extract.endLetter = fittingExtract.endLetter;
                                  lock (this.Pages) Pages.Add(CurrentPage);
                                  CurrentPage = this.GetFreshPage();
                                  FitWords(overflowExtract);
                              }
                              else
                              {
                                  CurrentPage.Extract.endLetter = extract.endLetter;
                              }
                          }

                          FitWords(new PageExtractDef() { startLetter = 0, endLetter = Content.Count - 1 });
                          lock (this.Pages) Pages.Add(CurrentPage);
                      });
            }
        }

        bool PosValid(PosDef Pos) => Content.Count > Pos.Letter && Pos.Letter >= 0;

        public override string ToString() => string.Join("", Content.Select(a => a.ToString()));

        public static Dictionary<FontWeight, Tuple<GlyphTypeface, GlyphTypeface>> Typefaces = new();
        private static readonly object lockObj = new();
        private static WordStyle GetStyle(BaseLinePart Part, CSSExtract CSS)
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
                    {
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
            lock (lockObj)
            {
                if (!Typefaces.ContainsKey(NewStyle.Weight))
                {
                    var tf = new Typeface(CharInfo.StandardFontFamily, FontStyles.Normal, NewStyle.Weight, new FontStretch(), CharInfo.StandardFallbackFontFamily);

                    if (!tf.TryGetGlyphTypeface(out GlyphTypeface typeface)) throw new Exception("Can't get glyph typeface");
                    var backupTf = new Typeface(CharInfo.StandardFallbackFontFamily, FontStyles.Normal, NewStyle.Weight, new FontStretch());
                    if (!backupTf.TryGetGlyphTypeface(out GlyphTypeface backupTypeface)) throw new Exception("Can't get backup glyph typeface");
                    Typefaces[NewStyle.Weight] = new(typeface, backupTypeface);
                }
            }
            return NewStyle;
        }

        internal int GetLocalPageCount(PosDef currPos, out bool inside)
        {
            inside = false;
            lock (this.Pages)
            {
                int count = 0;
                foreach (var Page in this.Pages)
                {
                    if (Page.StartPos > currPos)
                    {
                        inside = true;
                        return count;
                    }
                    count++;
                }
                return count;
            }
        }
        private void CreateContent(EpubPage page, CSSExtract CSS)
        {
            WordInfo wordInfo = new();
            bool lastImage = false;
            foreach (var Part in page.GetTextParts().SelectMany(a => a.Parts))
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
                            var letter = new TextLetter(Character, wordInfo);

                            bool NewWordBefore = TextPart.Splittable && CharInfo.PossibleLineBreaksBefore.Contains(Character);
                            bool NewWordAfter = TextPart.Splittable && CharInfo.PossibleLineBreaksAfter.Contains(Character);

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
            for (int i = 0; i < Content.Count; i++) if (this.Content[i].IsWordEnd) indexes.Add(i);
            List<Word> words = new(indexes.Count);

            foreach (var i in indexes)
            {
                extract.endLetter = i;
                words.Add(new Word(this.Content, extract));
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
                        letter.OwnWord.Prev = prev;
                        letter.OwnWord.Next = next;
                        letter.PrevLetter = prevLetter;
                        prevLetter = letter;
                    }
                }
            }
        }

        internal int GetPageCount()
        {
            lock (this.Pages) return this.Pages.Count;
        }

        internal RenderPage GetShownPage(PosDef position)
        {
            lock (this.Pages) return this.Pages.Find(a => a.Within(position));
        }

        internal int GetPageIndex(RenderPage shownPage)
        {
            lock (this.Pages) return this.Pages.IndexOf(shownPage);
        }

        internal RenderPage GetPage(int pageIndex, bool safe = false)
        {
            lock (this.Pages)
            {
                if (safe)
                {
                    if (pageIndex < 0) pageIndex = 0;
                    if (pageIndex >= this.Pages.Count) pageIndex = this.Pages.Count - 1;
                }
                return this.Pages[pageIndex];
            }
        }
    }
}

