using System.Collections.Generic;
using System.Windows;

namespace EPUBRenderer
{
    public static class GlobalSettings
    {
        //no brake space: " "
        public static Dictionary<char, SpecialCharacterInfo> VerticalVisualFixes = new Dictionary<char, SpecialCharacterInfo>()
            {
            {'」',new SpecialCharacterInfo('﹂',0, new Vector(0,0)) },
            {'「',new SpecialCharacterInfo('﹁',0, new Vector(0,0)) },
            {'（',new SpecialCharacterInfo('︵',0, new Vector(0,0)) },
            {'）',new SpecialCharacterInfo('︶',0, new Vector(0,0)) },
            {'『',new SpecialCharacterInfo('﹃',0, new Vector(-0,0)) },
            {'』',new SpecialCharacterInfo('﹄',0, new Vector(0,0)) },
            {'。',new SpecialCharacterInfo('︒',0, new Vector(0.32,-0.6)) },
            {'、',new SpecialCharacterInfo('、',-0.40, new Vector(0,-0.2)) },
            {'？',new SpecialCharacterInfo('?',0, new Vector(0.2,0)) },
            {'!',new SpecialCharacterInfo('!',0, new Vector(0,0)) },
            {'！',new SpecialCharacterInfo('!',0, new Vector(0.5,0)) },
            { 'ー',new SpecialCharacterInfo('│',0, new Vector(0,0.2)) },
            { '─',new SpecialCharacterInfo( '|', -0.25, new Vector(0,0)) },
            { '…',new SpecialCharacterInfo( '⋮', -0.41, new Vector(0,0)) }
        };

        public static char[] PossibleLineBreaks = ", .」』、?？！!を。─）:\n\r　\t".ToCharArray();
    }

    public class SpecialCharacterInfo
    {
        public char Replacement;
        //in parts of the font size
        public double WriteOffsetStart;
        public Vector RenderOffset;

        public SpecialCharacterInfo(char Replacement, double WriteOffsetStart, Vector RenderOffset)
        {
            this.Replacement = Replacement;
            this.WriteOffsetStart = WriteOffsetStart;         
            this.RenderOffset = RenderOffset;
        }
    }
}
