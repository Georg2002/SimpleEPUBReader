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
        public int Position()
        {
            return 0;
        }
        public Word()
        {
            Letters = new List<Letter>();
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
        public int Position()
        {
            return 0;
        }
        public Line()
        {
            Words = new List<Word>();
        }
        public override string ToString()
        {
            string Text = "";
            Words.ForEach(a => Text = Text + a);
            return Text;
        }
    }
}
