using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
    internal enum WordTypes
    {
        Normal, Ruby
    }

    internal class Word
    {
        public List<Letter> Letters;
        public WordTypes Type;
        public int Position(Word PrevWord, Vector PageSize, bool NewLine = false, bool TightFit = false, bool FinalRound = false)
        {
            Letter PrevLetter = null;
            if (PrevWord != null)
            {
                PrevLetter = PrevWord.Letters.Last();
            }
            int Fit = 0;

            LetterPlacementInfo Info = new LetterPlacementInfo()
            {
                PageSize = PageSize,
                PrevLetter = PrevLetter,
                OwnWord = this,
                PrevWord = PrevWord,
                NewLine = NewLine,
                TightFit = TightFit
            };
            bool AllFit = true;
            foreach (var Letter in Letters)
            {
                bool LetterFit = Letter.Position(Info);
                Info.NewLine = false;
                Info.PrevLetter = Letter;
                if (LetterFit)
                {
                    Fit++;
                }
                else
                {
                    AllFit = false;
                    break;
                }
            }

            if (FinalRound) return Fit;
            if (NewLine)
            {
                if (!AllFit)
                {
                    Fit = Position(PrevWord, PageSize, false, true);
                }
            }
            else if(!AllFit)
            {
                if (TightFit)
                {
                    Fit = Position(PrevWord, PageSize, false, true, true);
                }
                else
                {
                    Fit = Position(PrevWord, PageSize, true);
                }                
            }
           
          
            return Fit;
        }
        public Word()
        {
            Letters = new List<Letter>();
        }

        public Word(List<Letter> Letters, WordTypes Type)
        {
            this.Letters = Letters;
            this.Type = Type;
        }

        public override string ToString()
        {
            string Text = "";
            Letters.ForEach(a => Text = Text + a);
            return Text;
        }

    }

    internal class Line
    {
        public List<Word> Words;
        public Tuple<int, int> Position(Line Previous, Vector PageSize)
        {
            Word Prev = null;
            if (Previous != null)
            {
                Prev = Previous.Words.Last();
            }

            int WordFit = 0;
            int LetterFit = 0;
            foreach (var Word in Words)
            {
                
                LetterFit = Word.Position(Prev, PageSize);
                if (LetterFit<Word.Letters.Count)
                {
                    break;
                }
                WordFit++;
                Prev = Word;
            }
            return new Tuple<int, int>(WordFit, LetterFit);
        }

        public Tuple<Line, Line> Split(int WordCount, int LetterCount)
        {
            if (WordCount==0)
            {
                return new Tuple<Line, Line>(new Line(), new Line(Words));
            }
            var Front = Words.Take(WordCount+1).ToList();
            Front[Front.Count - 1] = new Word(Front.Last().Letters.Take(LetterCount).ToList(), Front.Last().Type);
            int k = 0;
            if (Words[WordCount-1].Letters.Count==LetterCount)
            {
                k = 1;
            }
            var Rear = Words.GetRange(WordCount+k, Words.Count - WordCount -k).ToList();
            if (k==0)
            {
                Rear[0] = new Word(Rear.First().Letters.GetRange(LetterCount, Rear.First().Letters.Count - LetterCount), Rear.First().Type);
            }
            
            return new Tuple<Line, Line>(new Line(Front), new Line(Rear));
        }


        public Line()
        {
            Words = new List<Word>();
        }
        public Line(List<Word> Words)
        {
            this.Words = Words;
        }

        public override string ToString()
        {
            string Text = "";
            Words.ForEach(a => Text = Text + a);
            return Text;
        }
    }
}
