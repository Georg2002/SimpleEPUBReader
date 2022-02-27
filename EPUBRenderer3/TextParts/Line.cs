using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
  

    internal class Line
    {
        public List<Word> Words;
        public Tuple<int, int> Position(Line Previous, Vector PageSize)
        {
            Word Prev = Previous?.Words.Last();
            
            int WordFit = 0;
            int LetterFit = 0;
            Word NextWord = null;
            for (int i = 0; i < Words.Count; i++)
            {
                var Word = Words[i];               
                NextWord = i == Words.Count - 1 ? null : Words[i + 1];
                LetterFit = Word.Position(Prev, NextWord, PageSize);
                if (LetterFit < Word.Letters.Count) break;              
                WordFit++;
                Prev = Word;
            }
            return new Tuple<int, int>(WordFit, LetterFit);
        }

        public Tuple<Line, Line> Split(int WordCount, int LetterCount)
        {
            if (WordCount == 0) return new Tuple<Line, Line>(new Line(), new Line(Words));       
            List<Word> Front;
            if (LetterCount == 0)                        Front = Words.Take(WordCount).ToList();
            else
            {
                Front = Words.Take(WordCount + 1).ToList();
                var FrontLast = Front.Last();               
                Front[Front.Count - 1] = new Word(FrontLast.Letters.Take(LetterCount).ToList(), FrontLast.Type, FrontLast.Style);               
            }

            int k = Words[WordCount].Letters.Count == LetterCount ? 1 : 0;
            var Rear = Words.GetRange(WordCount + k, Words.Count - WordCount - k).ToList();
            if (k == 0)
            {
                var RearFirst = Rear.First();              
                Rear[0] = new Word(RearFirst.Letters.GetRange(LetterCount, RearFirst.Letters.Count - LetterCount), RearFirst.Type, RearFirst.Style);
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
            Words.ForEach(a => Text += a);
            return Text;
        }
    }
}
