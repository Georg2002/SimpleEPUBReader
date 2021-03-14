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
        public int Position(Word Previous, Vector PageSize)
        {
            Letter PrevLetter = null;
            if (Previous != null)
            {
                PrevLetter = Previous.Letters.Last();
            }
            int Fit = 0;
            foreach (var Letter in Letters)
            {
                bool LetterFit = Letter.Position(Previous, PrevLetter,this, PageSize);
                PrevLetter = Letter;
                if (LetterFit)
                {
                    Fit++;
                }
               else
                {
                    break;
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
        public Tuple<int,int> Position(Line Previous, Vector PageSize)
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
                WordFit++;
                LetterFit = Word.Position(Prev, PageSize);
                Prev = Word;
                if (LetterFit < Word.Letters.Count)
                {
                    break;
                }
            }
            return new Tuple<int, int>(WordFit, LetterFit);
        }

        public Tuple<Line,Line> Split(int WordCount, int LetterCount)
        {
            var Front = Words.Take(WordCount).ToList();
            Front[Front.Count-1] = new Word( Front.Last().Letters.Take(LetterCount).ToList(), Front.Last().Type);
            var Rear = Words.GetRange(WordCount - 1, Words.Count - WordCount + 1).ToList();
            Rear[0] = new Word( Rear.First().Letters.GetRange(LetterCount, Rear.First().Letters.Count - LetterCount), Rear.First().Type);
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
