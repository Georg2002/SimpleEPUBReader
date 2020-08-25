using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer2
{
    public static class GlobalSettings
    {
        public static double NormalFontSize = 25;
        public static double RubyFontSize = 10;
        public static Brush NormalFontColor = Brushes.Black;
        public static FlowDirection NormalFlowDirection = FlowDirection.RightToLeft;
        public static Typeface NormalTypeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));

        public static Dictionary<char, char> VerticalVisualFixes = new Dictionary<char, char>()
            {
            {'」','﹂'},
            {'「','﹁'},
            {'（','︵'},
            {'）','︶'},
            {'『','﹃'},
            {'』','﹄'},
            {'。','︒'},
            {'、','︑'},
            {'？','?'},
            {'!','!'},
            {'！','!'},
            { 'ー','│'},
            { '─', '|'},
            { '…', '︙'}
        };
        public static char[] PossibleLineBreaks = ", .」』、?？！!を。─）):\n\r　\t".ToCharArray();
    }
}
