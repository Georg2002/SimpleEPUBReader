using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
        public static double FontSize = 25;
        //in times font size
        public static double LineSpace = 2;

        private static readonly Typeface Typeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));

        public WritingFlow Direction;

        private EpubPage Page;

        private List<LinePart> Parts;

        public ChapterPosition StartPos;
        public ChapterPosition EndPos;


        //CurrentWritePosition must always be at the point on the corner of the last added part that is closest to the limitations
        //VRTL: bottom left, VLTR: bottom right, HRTL: bottom left, HLTR: bottom right
        public ChapterPosition CurrentPos;

        public Point CurrentWritePosition;

        public double PageWidth;
        public double PageHeight;

        public PageRenderer()
        {
            Parts = new List<LinePart>();
        }


        public void SetContent(EpubPage Page)
        {
            this.Page = Page;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext Context)
        {
            var RubyFontSize = FontSize * 0.5;
            CurrentWritePosition = new Point();
            CurrentPos = StartPos;
            if (Page.PageSettings.Vertical)
            {
                if (Page.PageSettings.RTL)

                    Direction = WritingFlow.VRTL;
                else
                    Direction = WritingFlow.VLTR;
            }
            else
            {
                if (Page.PageSettings.RTL)

                    Direction = WritingFlow.HRTL;
                else
                    Direction = WritingFlow.HLTR;
            }
            bool LimitReached = false;

            for (CurrentPos.LineIndex = StartPos.LineIndex; CurrentPos.LineIndex < Page.Lines.Count; CurrentPos.LineIndex++)
            {
                var Line = Page.Lines[CurrentPos.LineIndex];
                for (CurrentPos.PartIndex = StartPos.PartIndex; CurrentPos.PartIndex < Line.Parts.Count; CurrentPos.PartIndex++)
                {
                    var Part = Line.Parts[CurrentPos.PartIndex];

                    LimitReached = AddPartIfPossible((TextLinePart)Part, Context);
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
        }

        //returns false if impossible
        public bool AddPartIfPossible(TextLinePart Part, DrawingContext Context)
        {
            bool Added = false;

            if (Part.Type == LinePartTypes.image)
            {
                DrawImage(Part, Context);
                return true;
            }

            TextLinePart DrawnPart = GetLargestFittingLinePart(Part);
            //Write(DrawnPart, Context);
            return Added;
        }

        //Tested and finished
        public TextLinePart GetLargestFittingLinePart(TextLinePart Part)
        {
            if (!string.IsNullOrEmpty(Part.Ruby) || Part.Type == LinePartTypes.sesame)
            {
                if (NeedsToWrap(Part.Text.Length))
                {
                    return new TextLinePart();
                }
                else
                {
                    return Part;
                }
            }
            
            TextLinePart Res = new TextLinePart("ERROR", "");
            if (Part.Text.Length < 20)
            {
                for (int i = Part.Text.Length; i >= CurrentPos.CharIndex; i--)
                {
                    int Length = i - CurrentPos.CharIndex;
                    if (!NeedsToWrap(Length))
                    {
                        Res = GetCutLinePart(Part, CurrentPos.CharIndex, Length);
                        break;
                    }
                }
            }
            else
            {
                bool LastFit = false;
                double LastPos = -1;
                int LastLength = -1;
                for (int i = 0; i < 100; i++)
                {
                    double Pos = GetQuickFindPos(i, CurrentPos.CharIndex, Part.Text.Length, LastFit, LastPos);
                    int Length = (int)Math.Round(Pos) - CurrentPos.CharIndex + 1;
                    LastFit = !NeedsToWrap(Length);
                    if (LastFit && Length == LastLength)
                    {
                        Res = GetCutLinePart(Part, CurrentPos.CharIndex, Length);
                        break;
                    }

                    LastPos = Pos;
                    LastLength = Length;
                }
            }

            if (Part.Text.Length == Res.Text.Length)
            {
                CurrentPos.CharIndex = 0;
            }            
            return Res;
        }

        //Tested and finished
        private TextLinePart GetCutLinePart(TextLinePart part, int charIndex, int length)
        {
            if (part.Text.Length > charIndex + length)
            {
                CurrentPos.CharIndex = charIndex + length;
            }
            else
            {
                CurrentPos.CharIndex  = 0;
            }
            return new TextLinePart(part.Text.Substring(charIndex, length), "");
        }

        //Tested and finished
        public double GetQuickFindPos(int Iteration, int charIndex, int TotalLength, bool LastFit, double LastPos)
        {
            if (LastPos == -1)
            {
                return TotalLength - 1;
            }
            double Length = TotalLength - charIndex;

            double Change = Length / Math.Pow(2, Iteration);
            if (LastFit)
            {
                return LastPos + Change;
            }
            else
            {
                return LastPos - Change;
            }
        }

        private void DrawImage(LinePart Part, DrawingContext Context)
        {
            var Source = ((ImageLinePart)Part).GetImage();
            if (Source == null)
            {
                Context.DrawRectangle(Brushes.Red, null, new Rect(0, 0, PageWidth, PageHeight));
            }
            else
            {
                Context.DrawImage(Source, new Rect(0, 0, PageWidth, PageHeight));
            }
        }

        /*  private void Write(LinePart C, DrawingContext Context)
          {
              Point MainWritingPosOffset = new Point();
              Point UpperWritingPosOffset = new Point();
              switch (Direction)
              {
                  case WritingFlow.VRTL:
                      MainWritingPosOffset.X = -FontSize / 2;
                      break;
                  case WritingFlow.VLTR:
                      MainWritingPosOffset.X = FontSize * (LineSpace - 0.5);
                      break;
                  default:
                      throw new NotImplementedException();
              }

              string Text = "";
              if (Page.PageSettings.Vertical)
              {
                  if (GlobalSettings.VerticalVisualFixes.ContainsKey(C))
                  {
                      var Fix = GlobalSettings.VerticalVisualFixes[C];
                      Text = Fix.Replacement;
                  }
                  else
                  {
                      Text = C.ToString();
                  }
                  var MainFormattedText = new FormattedText(Text, CultureInfo.InvariantCulture,
                   FlowDirection.RightToLeft, Typeface, FontSize, Brushes.Black, 1);
                  Context.DrawText(MainFormattedText, CurrentWritePosition);
              }
          }
        */

        private FormattedText GetFormattedText(string Text, double FontSize)
        {
            FlowDirection flowDirection;
            if (Page.PageSettings.RTL)
            {
                flowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                flowDirection = FlowDirection.LeftToRight;
            }

            return new FormattedText(Text, CultureInfo.InvariantCulture,
                flowDirection, Typeface, FontSize, Brushes.Black, 1);
        }

        //Tested and unfinished
        public void Wrap()
        {
            switch (Direction)
            {
                //    case WritingFlow.HLTR:
                //        CurrentWritePosition.X = FontSize / 2;
                //        CurrentWritePosition.Y = CurrentWritePosition.Y + FontSize * LineSpace;
                //        break;
                //    case WritingFlow.HRTL:
                //        CurrentWritePosition.X = PageWidth - FontSize / 2;
                //        CurrentWritePosition.Y = CurrentWritePosition.Y + FontSize * LineSpace;
                //        break;

                case WritingFlow.VLTR:
                    CurrentWritePosition.X = CurrentWritePosition.X + FontSize * LineSpace;
                    CurrentWritePosition.Y = 0;
                    break;
                case WritingFlow.VRTL:
                    CurrentWritePosition.X = CurrentWritePosition.X - FontSize * LineSpace;
                    CurrentWritePosition.Y = 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool NeedsToWrap(int NewCharacters)
        {
            switch (Direction)
            {
                default:
                    throw new NotImplementedException();
                //    case WritingFlow.HLTR:
                //        return CurrentWritePosition.X + FontSize * (NewCharacters - 0.5) > PageWidth;
                //    case WritingFlow.HRTL:
                //        return CurrentWritePosition.X - FontSize * (NewCharacters - 0.5) < 0;
                case WritingFlow.VLTR:
                    return CurrentWritePosition.Y + FontSize * NewCharacters > PageHeight;
                case WritingFlow.VRTL:
                    return CurrentWritePosition.Y + FontSize * NewCharacters > PageHeight;
            }
        }

        public bool PageFull()
        {
            switch (Direction)
            {
                case WritingFlow.VLTR:
                    return CurrentWritePosition.X + FontSize * LineSpace > PageWidth;
                case WritingFlow.VRTL:
                    return CurrentWritePosition.X - FontSize * LineSpace < 0;
                default:
                    throw new NotImplementedException();
            }
        }

        //   private void RenderVRTL(DrawingContext Context)
        //   {
        //       double TopFreeSpace = 5;
        //
        //       Width = FontSize * 1.45;
        //       var MainFormattedText = new FormattedText(VMainText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Brushes.Black, 1);
        //       MainFormattedText.LineHeight = FontSize;
        //       Height = MainFormattedText.LineHeight * MainText.Length + TopFreeSpace;
        //       MainFormattedText.TextAlignment = TextAlignment.Center;
        //       var RubyFontSize = FontSize * 0.5;
        //       var UpperFormattedText = new FormattedText(VUpperText, CultureInfo.InvariantCulture, FlowDirection.RightToLeft, Typeface, RubyFontSize, Brushes.Black, 1);
        //       UpperFormattedText.LineHeight = Height / UpperText.Length;
        //       UpperFormattedText.LineHeight = Math.Max(UpperFormattedText.LineHeight, RubyFontSize);
        //       UpperFormattedText.TextAlignment = TextAlignment.Center;
        //
        //       Context.DrawText(MainFormattedText, new Point(FontSize * 0.5, TopFreeSpace));
        //       Context.DrawText(UpperFormattedText, new Point(FontSize * 1.2, -FontSize / 8 + TopFreeSpace));
        //   }

        public enum WritingFlow
        {
            VRTL, VLTR, HRTL, HLTR
        }
    }
}
