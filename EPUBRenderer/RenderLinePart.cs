using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using EPUBParser;

namespace EPUBRenderer
{
    public class RenderLinePart : FrameworkElement
    {
        private static int FontSize = 35;

        private static DirectionDefiner Direction = DirectionDefiner.VRTL;

        private static EpubSettings _PageSettings;
        public static EpubSettings PageSettings
        {
            get => _PageSettings; set { _PageSettings = value; SetDirectionDefiner(); }
        }

        private LinePart _Part;
        public LinePart Part
        {
            get => _Part;
            set
            {
                _Part = value;
                SetContent();
                InvalidateVisual();
            }
        }

        private string MainText;
        private string VMainText;
        private string UpperText;
        private string VUpperText;
        private System.Drawing.Image Image;

        private void SetContent()
        {
            if (Part.Type == LinePartTypes.image)
            {
                throw new NotImplementedException();
            }
            else
            {
                var TextPart = (TextLinePart)Part;
                MainText = TextPart.Text;
                VMainText = MakeVertical(MainText);
                if (Part.Type == LinePartTypes.sesame)
                {
                    UpperText = new string('﹅', MainText.Length);
                }
                else
                {
                    UpperText = TextPart.Ruby;
                }
                VUpperText = MakeVertical(UpperText);
            }
        }

        private string MakeVertical(string In)
        {
            string Out = "";
            for (int i = 0; i < In.Length; i++)
            {
                if (i != 0)
                {
                    Out += '\n';
                }
                Out += In[i];
            }
            return Out;
        }

        protected override void OnRender(DrawingContext Context)
        {
            if (Part.Type == LinePartTypes.image)
            {

            }
            else
            {
                switch (Direction)
                {
                    case DirectionDefiner.HLTR:
                        break;
                    case DirectionDefiner.HRTL:
                        break;
                    case DirectionDefiner.VLTR:
                        break;
                    case DirectionDefiner.VRTL:
                        RenderVRTL(Context);
                        break;
                }
            }


            //    Context.DrawLine(new Pen(Brushes.Blue, 2.0),
            //        new Point(0.0, 0.0),
            //        new Point(ActualWidth, ActualHeight));
            //    Context.DrawLine(new Pen(Brushes.Green, 2.0),
            //        new Point(ActualWidth, 0.0),
            //        new Point(0.0, ActualHeight));
        }

        private void RenderVRTL(DrawingContext Context)
        {
            Width = FontSize * 1.5;
            var Stretch = new FontStretch();
            var Typeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal, FontWeights.Normal, Stretch);
            var MainFormattedText = new FormattedText(VMainText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Brushes.Black, 1);
            MainFormattedText.LineHeight = FontSize;
            Height = MainFormattedText.LineHeight * MainText.Length;
            MainFormattedText.TextAlignment = TextAlignment.Center;
            var RubyFontSize = FontSize * 0.5;
            var UpperFormattedText = new FormattedText(VUpperText, CultureInfo.InvariantCulture, FlowDirection.RightToLeft, Typeface, RubyFontSize, Brushes.Black, 1);
            UpperFormattedText.LineHeight = Height / UpperText.Length;
            UpperFormattedText.LineHeight = Math.Max(UpperFormattedText.LineHeight, RubyFontSize);
            UpperFormattedText.TextAlignment = TextAlignment.Center;

         //   Context.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.Red, 3), new Rect(0, 0, Width, Height));
            Context.DrawText(MainFormattedText, new Point(FontSize * 0.5, 0));
            Context.DrawText(UpperFormattedText, new Point(FontSize * 1.2, -FontSize / 8));
        }

        private static void SetDirectionDefiner()
        {
            int i = 0;
            if (PageSettings.Vertical)
            {
                i = 2;
            }
            if (PageSettings.RTL)
            {
                i++;
            }
            Direction = (DirectionDefiner)i;
        }

        private enum DirectionDefiner
        {
            HLTR, HRTL, VLTR, VRTL
        }
    }
}
