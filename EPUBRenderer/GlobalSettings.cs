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
        public static Dictionary<char, string> VerticalVisualFixes = new Dictionary<char, string>()
            {
            {'」',"  」" },   {'「',"「   " },   {'『',"『   " }, {'』',"  』" },   {'。',"  。" },   {'、',"   、" },   {'?',"  ?" }, { 'ー', "ｌ"}
            };
    }  
}
