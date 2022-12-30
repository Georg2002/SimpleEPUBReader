using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System;
using System.Windows.Media;

namespace EPUBRenderer3
{
    internal enum WordTypes
    {
        Normal, Ruby
    }

    internal class WordStyle
    {
        public FontWeight Weight;
        public float RelativeFontSize;
        public double? Width = null;
        public double? Height = null;
        public Typeface Typeface;
        public WordStyle()
        {
            Weight = FontWeights.Normal;
            RelativeFontSize = 1;
            this.Typeface = new Typeface(CharInfo.StandardFont, FontStyles.Normal,
            Weight, new FontStretch(), CharInfo.StandardFallbackFont);
        }
    }

    internal class Word
    {
        public List<Letter> Letters;
        public WordTypes Type;
        public WordStyle Style;
        private LetterPlacementInfo Info = new LetterPlacementInfo();//less garbage collection
        
        public int Position(Word PrevWord, Word NextWord, Vector PageSize, bool NewLine = false, bool TightFit = false, bool FinalRound = false)
        {
            Letter PrevLetter = null;
            if (PrevWord != null) PrevLetter = PrevWord.Letters.Last();
            int Fit = 0;
            this.Info.PageSize = PageSize;
            this.Info.PrevLetter = PrevLetter;
            this.Info.OwnWord = this;
            this.Info.PrevWord = PrevWord;
            this.Info.NextWord = NextWord;
            this.Info.NewLine = NewLine;
            this.Info.TightFit = TightFit;
            this.Info.Last = false;

            bool AllFit = true;
            for (int i = 0; i < Letters.Count; i++)
            {
                var Letter = Letters[i];
                Info.Last = i == Letters.Count - 1;
                Letter.PrevLetter = Info.PrevLetter;
                bool LetterFit = Letter.Position(Info);
                Info.NewLine = false;
                Info.PrevLetter = Letter;
                if (LetterFit) Fit++;
                else
                {
                    AllFit = false;
                    break;
                }
            }
            if (Fit != 0 && !Letters[Fit - 1].InsidePageHor(PageSize)) return 0;
            if (FinalRound) return Fit;
            if(!AllFit)
            {
                if (NewLine) Fit = Position(PrevWord, NextWord, PageSize, NewLine: false, TightFit: true);            
                else
                {
                    if (TightFit) Fit = Position(PrevWord, NextWord, PageSize, NewLine: false, TightFit: true, FinalRound: true);
                    else Fit = Position(PrevWord, NextWord, PageSize, true);
                }
            }
           

            return Fit;
        }

        public static Tuple<int, int> PositionWords(List<Word> words, Vector PageSize)
        {
            Word Prev = null;

            int WordFit = 0;
            int LetterFit = 0;
            Word NextWord = null;
            for (int i = 0; i < words.Count; i++)
            {
                var Word = words[i];
                WordFit++;
                NextWord = i == words.Count - 1 ? null : words[i + 1];
                LetterFit = Word.Position(Prev, NextWord, PageSize);
                if (LetterFit < Word.Letters.Count) break;
                LetterFit = 0;
                Prev = Word;
            }
            return new Tuple<int, int>(WordFit, LetterFit);
        }

        internal Tuple<Word, Word> Split(int letterCount)
        {
            var front = new Word(this.Letters.Take(letterCount).ToList(), this.Type, this.Style);
            var rear = new Word(this.Letters.GetRange(letterCount, this.Letters.Count - letterCount).ToList(), this.Type, this.Style);
            return new Tuple<Word, Word>(front, rear);
        }

        public static Tuple<List<Word>, List<Word>> SplitWords(List<Word> words, int WordCount, int LetterCount)
        {
            //Word count is words including partial, letter count are the letters of the last (maybe partial) word
            List<Word> front = new List<Word>();
            List<Word> rear = new List<Word>();
            //at first only the full words are added, then the partial word is split and added
            int totalCount = words.Count;
            front.AddRange(words.Take(WordCount - 1));
            if (WordCount < totalCount) rear.AddRange(words.GetRange(WordCount, totalCount - WordCount));//avoid out of range when the last word is partial          

            var partWord = words[WordCount - 1];
            (Word frontWord, Word rearWord) = partWord.Split(LetterCount);
            if (frontWord.Letters.Count > 0) front.Add(frontWord);
            if (rearWord.Letters.Count > 0) rear.Insert(0, rearWord);

            return new Tuple<List<Word>, List<Word>>(front, rear);
        }

        public Word() => Letters = new List<Letter>();

        public bool Visible()
        {
            foreach (var Letter in Letters) if (Letter.Type != LetterTypes.Break) return true;
            return false;
        }

        public Word(List<Letter> Letters, WordTypes Type, WordStyle Style)
        {
            this.Letters = Letters;
            this.Type = Type;
            this.Style = Style;
        }

        public override string ToString()
        {
            string Text = "";
            Letters.ForEach(a => Text += a);
            return Text;
        }

        public double Length() => Letters.Last().EndPosition.Y - Letters.First().StartPosition.Y;
    }
}
