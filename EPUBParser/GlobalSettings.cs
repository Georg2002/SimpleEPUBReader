using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

        public static string[] PackageFileNames = new string[] { "package.opf", "content.opf" };

        public static string[] PossibleImageSourceNames = new string[] { "src", "href", "xlink:href" };

        public static string[] IgnoreableSpanClassParts = new string[]
        {
            "calibre"
        };
    }
}
