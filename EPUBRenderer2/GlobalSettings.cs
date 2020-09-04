using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public static class GlobalSettings
    {
        public static double NormalFontSize = 20;
        public static double RubyFontSize = NormalFontSize * 0.6;
        public static double LineHeight = NormalFontSize * 1.7;
        public static double RubyOffset = NormalFontSize * 1.05;
        public static Brush NormalFontColor = Brushes.Black;
        public static FlowDirection NormalFlowDirection = FlowDirection.RightToLeft;
        public static Typeface NormalTypeface = new Typeface(new FontFamily("Hiragino Sans GB W3"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));
        public static Rect ErrorRect = new Rect(0, 0, 500, 500);

        public static Dictionary<char, char> VerticalVisualFixes = new Dictionary<char, char>()
            {
            {'」','﹂'},{'「','﹁'},{'（','︵'},
            {'）','︶'},{'『','﹃'},{'』','﹄'},
            {'。','︒'},{'、','︑'},{'？','?'},
            {'!','!'},{'！','!'},{ 'ー','│'},
            { '─', '|'},{ '…', '︙'},{'〈','︿' },
            {'〉','﹀' },{'【','︻' },{'】','︼' },
            {'≪','︽' },{'≫','︾' },{'《','︽' },
            {'》','︾' },{'(','︵' },{')','︶' },
            {'→','↓'},{'：','‥'}
        };
        public static char[] PossibleLineBreaks = ", .」』、?？！!を。─）〉):\n\r　\t】≫》".ToCharArray();

        public static void SetNightmode(bool nightmode)
        {
            if (nightmode)
            {
                NormalFontColor = new SolidColorBrush(Colors.White)
                {
                    Opacity = 0.6
                };
            }
            else
            {
                NormalFontColor = Brushes.Black;
            }
        }
    }
}
