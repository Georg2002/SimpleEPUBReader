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
        private static int FontSize = 15;

        private static DirectionDefiner Direction = DirectionDefiner.VRTL;

        private static EpubSettings _PageSettings;
        public static EpubSettings PageSettings
        {
            get => _PageSettings; set { _PageSettings = value; SetDirectionDefiner();  }
        }       

        private LinePart _Part;
        public LinePart Part
        {
            get => _Part; set { InvalidateVisual(); _Part = value; }
        }

        protected override void OnRender(DrawingContext Context)
        {
            if (Part.Type == LinePartTypes.image)
            {

            }
            else
            {
                var TextPart = (TextLinePart)Part;                
                string MainText = TextPart.Text;
                string UpperText = "";

                if (Part.Type == LinePartTypes.sesame)
                {
                    UpperText = new string('﹅', MainText.Length);
                }
                else
                {
                    UpperText = TextPart.Ruby;
                }

                switch (Direction)
                {
                    case DirectionDefiner.HLTR:
                        break;
                    case DirectionDefiner.HRTL:
                        break;
                    case DirectionDefiner.VLTR:
                        break;
                    case DirectionDefiner.VRTL:
                        RenderVRTL(Context, MainText, UpperText);
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

        private void RenderVRTL(DrawingContext Context, string MainText, string UpperText)
        {
            Height = FontSize * 2;
            Width = FontSize * MainText.Length;
            var MainFormattedText = new FormattedText(MainText,
                CultureInfo.CreateSpecificCulture("jp"), FlowDirection.RightToLeft, new Typeface("calibre"), FontSize,Brushes.Black,1.25);
            Context.DrawText(MainFormattedText, new Point(10, 10));
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
