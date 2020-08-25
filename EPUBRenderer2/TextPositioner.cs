using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer2
{
    public static class TextPositioner
    {
        private static EpubSettings PageSettings;
        private static Vector PageSize;

        public static void Position(List<List<TextElement>> ElementString, Vector PageSize, EpubSettings PageSettings)
        {
            TextPositioner.PageSettings = PageSettings;
            TextPositioner.PageSize = PageSize;
            Vector CurrentWritePosition = 

            for (int i = 0; i < ElementString.Count; i++)
            {
                var CurrentWord = ElementString[i];
                var First = CurrentWord.First();
                switch (First.ElementType)
                {
                    case TextElementType.Letter:
                        break;
                    case TextElementType.RubyLetter:
                        break;
                    case TextElementType.Image:
                        break;
                    case TextElementType.Break:
                        break;                    
                }
            }
        }
    }
}
