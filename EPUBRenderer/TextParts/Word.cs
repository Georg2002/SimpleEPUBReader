using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System;
using System.Windows.Media;
using System.Text;

namespace EPUBRenderer
{
    internal enum WordTypes
    {
        Normal, Ruby
    }

    internal class WordStyle
    {
        public FontWeight Weight = FontWeights.Normal;
        public float RelativeFontSize = 1;
        public double? Width = null;
        public double? Height = null;
        public Typeface Typeface;       
    }

    internal class Word
    {
        public IEnumerable<Letter> Letters;
        public int LetterCount => Extract.length;
        public PageExtractDef Extract;
        public WordTypes Type;
        public Word(IEnumerable<Letter> Content, PageExtractDef extract)
        {
            Letters = Content.GetExtract(extract);
            Extract = extract;
            Letter first = Letters.First();
            Type = first.IsRuby ? WordTypes.Ruby : WordTypes.Normal;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(LetterCount);
            foreach (var l in Letters) sb.Append(l.ToString());
            return sb.ToString();
        }

        public double Length() => Letters.Last().EndPosition.Y - Letters.First().StartPosition.Y;
        /*
          public static int PositionWords(List<Word> words, Vector PageSize)
        {
            Letter Prev = null;

            int WordFit = 0;
            int LetterFit = 0;
            for (int i = 0; i < words.Count; i++)
            {
                var Word = words[i];
                WordFit++;
                LetterFit = Word.Position(Prev, PageSize);
                if (LetterFit < Word.Letters.Count) break;
                LetterFit = 0;
                Prev = Word.Letters.Last();
            }
            return LetterFit;
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
        */
    }
}
