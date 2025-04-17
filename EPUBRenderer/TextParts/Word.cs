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
    }

    internal class Word
    {
        public IEnumerable<Letter> Letters;
        public int LetterCount => this.Extract.Length;
        public PageExtractDef Extract;
        public WordTypes Type;
        internal Word Prev;
        internal Word Next;

        public Word(IEnumerable<Letter> Content, PageExtractDef extract)
        {
            Letters = Content.GetExtract(extract);
            Extract = extract;
            Letter first = this.Letters.First();
            Type = first.IsRuby ? WordTypes.Ruby : WordTypes.Normal;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(this.LetterCount);
            foreach (var l in Letters) sb.Append(l.ToString());
            return sb.ToString();
        }

        public double Length() => this.Letters.Last().EndPosition.Y - this.Letters.First().StartPosition.Y;
    }
}
