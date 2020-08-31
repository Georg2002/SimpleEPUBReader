using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBReader
{
    public static class GlobalSettings
    {    
        public static bool Nightmode;
        public static Style CurrentButtonStyle;

        public static  string GetSaveFolderPath()
        {
            var AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(AppData, "SimpleEpubReader");
        }
        public const string SaveFileName = "save.xml";
    }
}
