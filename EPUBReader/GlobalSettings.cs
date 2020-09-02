using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBReader
{
    public static class GlobalSettings
    {    
        public static bool Nightmode;
        public static bool LeaveMenuDown;
        public static Style CurrentButtonStyle;

        public static  string GetSaveFolderPath()
        {
            var AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(AppData, "SimpleEpubReader");
        }
        public const string SaveFileName = "save.xml";

        private const int Alpha = 100;
        public static Color[] MarkingColors =
            new Color[]
            {
                new Color(){R=255, G=0,B=0,A =Alpha},
                new Color(){R=0, G=255,B=0,A =Alpha},
                new Color(){R=255, G=255,B=0,A =Alpha},
                new Color(){R=0, G=0,B=255,A =Alpha}
            };
    }
}
