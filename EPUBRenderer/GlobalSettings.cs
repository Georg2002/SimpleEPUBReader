using CefSharp.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader
{
    public static class GlobalSettings
    {
        //no brake space: " "
        public static Dictionary<char, SpecialCharacterInfo> VerticalVisualFixes = new Dictionary<char, SpecialCharacterInfo>()
            {
            {'」',new SpecialCharacterInfo("  」",0,0) },
            {'「',new SpecialCharacterInfo("「   " ,0,0)},
            {'『',new SpecialCharacterInfo("『   ",0,0) },
            {'』',new SpecialCharacterInfo("  』" ,0,0)},
            {'。',new SpecialCharacterInfo("  。" ,0,0)},
            {'、',new SpecialCharacterInfo("   、" ,0,0)},
            {'?',new SpecialCharacterInfo("  ?",0,0) },
            { 'ー',new SpecialCharacterInfo( "ｌ",0,0)}
            };
    }

    public class SpecialCharacterInfo
    {
        public string Replacement;
        //in parts of the font size,positive means up
        public double StartOffset;
        public double EndOffset;
        public SpecialCharacterInfo(string Replacement, double StartOffset, double EndOffset)
        {
            this.Replacement = Replacement;
            this.StartOffset = StartOffset;
            this.EndOffset = EndOffset;
        }
    }
}
