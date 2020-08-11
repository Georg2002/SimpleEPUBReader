using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    internal static class GlobalSettings
    {
        public static string[] VerticalLanguages = new string[] { "ja" };
        public static bool IsVerticalLanguage(string Language)
        {
            return VerticalLanguages.Contains(Language);
        }
    }
}
