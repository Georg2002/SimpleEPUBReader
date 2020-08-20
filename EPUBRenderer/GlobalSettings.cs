using CefSharp.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
{
    public static class GlobalSettings
    {
        //no brake space: " "
        public static Dictionary<char, SpecialCharacterInfo> VerticalVisualFixes = new Dictionary<char, SpecialCharacterInfo>()
            {
            {'」',new SpecialCharacterInfo('」', new Vector(0,0), new Vector(0,0)) },
            {'「',new SpecialCharacterInfo('「', new Vector(0,0), new Vector(0,0)) },
            {'『',new SpecialCharacterInfo('『', new Vector(0,0), new Vector(0,0)) },
            {'』',new SpecialCharacterInfo('』', new Vector(0,0), new Vector(0,0)) },
            {'。',new SpecialCharacterInfo('。', new Vector(0,0), new Vector(0,0)) },
            {'、',new SpecialCharacterInfo('、', new Vector(0,0), new Vector(0,0)) },
            {'?',new SpecialCharacterInfo('?'    , new Vector(0,0), new Vector(0,0)) },
            { 'ー',new SpecialCharacterInfo('ー' , new Vector(0,0), new Vector(0,0)) },
            { '─',new SpecialCharacterInfo( '─'  , new Vector(0,0), new Vector(0,0)) }
        };

        public static char[] PossibleLineBreaks = ", .」?！。─".ToCharArray();
    }

    public class SpecialCharacterInfo
    {
        public char Replacement;
        //in parts of the font size
        public Vector StartOffset;
        public Vector EndOffset;

        public SpecialCharacterInfo(char Replacement, Vector StartOffset, Vector EndOffset)
        {
            this.Replacement = Replacement;
            this.StartOffset = StartOffset;
            this.EndOffset = EndOffset;
        }
    }
}
