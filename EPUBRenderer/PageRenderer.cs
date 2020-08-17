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
using EPUBReader;

namespace EPUBRenderer
{
    public class PageRenderer : FrameworkElement
    {
        private static int FontSize = 25;      

        private static readonly Typeface Typeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));      
      
        private string MainText;
        private string VMainText;
        private string UpperText;
        private string VUpperText;

        private EpubPage Page;

        private List<LinePart> Parts;

        public ChapterPosition StartPos;
        public ChapterPosition EndPos;

        private ChapterPosition CurrentPos;

        private Point CurrentWritePosition;

        public PageRenderer()
        {
            Parts = new List<LinePart>();
        }
            

        public void SetContent(EpubPage Page)
        {
            this.Page = Page;
            InvalidateVisual();
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
                char Addition = In[i];
                if (GlobalSettings.VerticalVisualFixes.ContainsKey(Addition))
                {
                    Out += GlobalSettings.VerticalVisualFixes[Addition];
                }
                else
                {
                    Out += Addition;
                }
            }
            return Out;
        }    

        protected override void OnRender(DrawingContext Context)
        {
            CurrentWritePosition = new Point();
            CurrentPos = new ChapterPosition();

            bool LimitReached = false;

            for (CurrentPos.LineIndex = StartPos.LineIndex; CurrentPos.LineIndex < Page.Lines.Count; CurrentPos.LineIndex++)
            {
                var Line = Page.Lines[CurrentPos.LineIndex];
                for (CurrentPos.PartIndex = StartPos.PartIndex; CurrentPos.PartIndex < Line.Parts.Count; CurrentPos.PartIndex++)
                {
                    var Part = Line.Parts[CurrentPos.PartIndex];
                    LimitReached = AddPartIfPossible(Part, Context);
                    if (LimitReached)
                        break;
                }
                if (LimitReached)
                    break;
            }

            EndPos = CurrentPos;

            //   Context.DrawLine(new Pen(Brushes.Blue, 2.0),
            //       new Point(0.0, 0.0),
            //       new Point(ActualWidth, ActualHeight));
            //   Context.DrawLine(new Pen(Brushes.Green, 2.0),
            //       new Point(ActualWidth, 0.0),
            //       new Point(0.0, ActualHeight));
        }  //

        //returns false if impossible
        private bool AddPartIfPossible(LinePart Part, DrawingContext Context)
        {
            return false;
           // if (Part.Type == LinePartTypes.image)
           // {
           //     var Source = ((ImageLinePart)Part).GetImage();
           //     Context.DrawImage(Source, new Rect(0, 0, ActualWidth, ActualHeight));
           // }
           // else
           // {
           //     foreach (char C in Part.Text)
           //     {
           //
           //     }
           // }
        }


        private void RenderVRTL(DrawingContext Context)
        {
            double TopFreeSpace = 5;

            Width = FontSize * 1.45;
            var MainFormattedText = new FormattedText(VMainText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Brushes.Black, 1);
            MainFormattedText.LineHeight = FontSize;
            Height = MainFormattedText.LineHeight * MainText.Length + TopFreeSpace;
            MainFormattedText.TextAlignment = TextAlignment.Center;
            var RubyFontSize = FontSize * 0.5;
            var UpperFormattedText = new FormattedText(VUpperText, CultureInfo.InvariantCulture, FlowDirection.RightToLeft, Typeface, RubyFontSize, Brushes.Black, 1);
            UpperFormattedText.LineHeight = Height / UpperText.Length;
            UpperFormattedText.LineHeight = Math.Max(UpperFormattedText.LineHeight, RubyFontSize);
            UpperFormattedText.TextAlignment = TextAlignment.Center;

            Context.DrawText(MainFormattedText, new Point(FontSize * 0.5, TopFreeSpace));
            Context.DrawText(UpperFormattedText, new Point(FontSize * 1.2, -FontSize / 8 + TopFreeSpace));
        }
    }
}
