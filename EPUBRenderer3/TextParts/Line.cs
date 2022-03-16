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
                WordFit++;
                NextWord = i == Words.Count - 1 ? null : Words[i + 1];
                LetterFit = Word.Position(Prev, NextWord, PageSize);
                if (LetterFit < Word.Letters.Count) break;
                LetterFit = 0;
                Prev = Word;
            }
            return new Tuple<int, int>(WordFit, LetterFit);
        }

        public Tuple<Line, Line> Split(int WordCount, int LetterCount)
        {
            //Word count is words including partial, letter count are the letters of the last (maybe partial) word
            List<Word> front = new List<Word>();
            List<Word> rear = new List<Word>();
            //at first only the full words are added, then the partial word is split and added
            int totalCount = Words.Count;
            front.AddRange(Words.Take(WordCount-1));
            if (WordCount<totalCount) rear.AddRange(Words.GetRange(WordCount, totalCount - WordCount));//avoid out of range when the last word is partial          
            
            var partWord = Words[WordCount-1];
            (Word frontWord , Word rearWord) = partWord.Split(LetterCount);
            if (frontWord.Letters.Count > 0) front.Add(frontWord);
            if (rearWord.Letters.Count > 0) rear.Insert(0, rearWord);


            return new Tuple<Line, Line>(new Line(front), new Line(rear));





            if (WordCount == 0 && LetterCount == 0) return new Tuple<Line, Line>(new Line(), new Line(Words));

            List<Word> Front;
            if (LetterCount == 0) Front = Words.Take(WordCount).ToList();
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
