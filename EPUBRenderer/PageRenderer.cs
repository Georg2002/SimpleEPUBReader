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
    public class PageRenderer : FrameworkElement
    {
        public static double FontSize = 25;
        //all in times font size
        public static double LineSpace = 2;
        public static double RubyFontSize = 0.5;
        public static double RubyOffSet = 1.5;

        public WritingDirection WritingDirection;
        public WritingFlow Direction;

        public EpubPage Page;
        public ChapterPosition StartPos;
        public ChapterPosition EndPos;

        public static readonly Typeface Typeface = new Typeface(new FontFamily("Hiragino Sans GB W6"), FontStyles.Normal,
            FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));

        //CurrentWritePosition must always be at the point on the corner of the last added part that is closest to the limitations
        //VRTL: bottom left, VLTR: bottom right, HRTL: bottom left, HLTR: bottom right
        public ChapterPosition CurrentPos;

        public Point CurrentWritePosition;

        public double PageWidth;
        public double PageHeight;


        private List<FormattedText> TextParts;
        private List<Point> TextPartPositions;

        private ImageSource Image;

        private bool IsImagePage;




        public PageRenderer()
        {
            TextParts = new List<FormattedText>();
            TextPartPositions = new List<Point>();
            WritingDirection = new WritingDirection(this);
        }

        protected override void OnRender(DrawingContext Context)
        {
            Height = PageHeight;
            Width = PageWidth;
            if (IsImagePage)
            {
                DrawImage(Context);
            }
            else
            {
                for (int i = 0; i < TextParts.Count; i++)
                {
                    var Text = TextParts[i];
                    var Pos = TextPartPositions[i];
                    Context.DrawText(Text, Pos);
                }
            }
            Context.DrawLine(new Pen(Brushes.Blue, 2.0),
                new Point(0.0, 0.0),
                new Point(ActualWidth, ActualHeight));
            Context.DrawLine(new Pen(Brushes.Green, 2.0),
                new Point(ActualWidth, 0.0),
                new Point(0.0, ActualHeight));
        }

        private void DrawImage(DrawingContext Context)
        {
            if (Image == null)
            {
                Context.DrawRectangle(Brushes.Red, null, new Rect(0, 0, PageWidth, PageHeight));
            }
            else
            {
                double ImageRatio = Image.Width / Image.Height;
                double ScreenRatio = PageWidth / PageHeight;
                double DrawHeight = 0;
                double DrawWidth = 0;
                double OffsetX = 0;
                double OffsetY = 0;
                if (ImageRatio > ScreenRatio)
                {
                    DrawWidth = PageWidth;
                    DrawHeight = PageWidth / ImageRatio;
                    OffsetX = 0;
                    OffsetY = (PageHeight - DrawHeight) / 2;
                }
                else
                {
                    DrawWidth = PageHeight * ImageRatio;
                    DrawHeight = PageHeight;
                    OffsetX = (PageWidth - DrawWidth) / 2;
                    OffsetY = 0;
                }
                
                Context.DrawImage(Image, new Rect(OffsetX,OffsetY, DrawWidth, DrawHeight));
            }
        }

        public void SetContent(EpubPage Page)
        {
            this.Page = Page;
            Image = null;
            IsImagePage = false;
            TextParts.Clear();
            TextPartPositions.Clear();
            CurrentWritePosition = WritingDirection.GetPageStartPos();
            WritingDirection.Wrap();
            CurrentPos = StartPos;
            SetDirection();

            bool LimitReached = false;

            while (CurrentPos.LineIndex < Page.Lines.Count)
            {
                var Line = Page.Lines[CurrentPos.LineIndex];
                while (CurrentPos.PartIndex < Line.Parts.Count)
                {
                    var Part = Line.Parts[CurrentPos.PartIndex];
                    if (Part.Type == LinePartTypes.image)
                    {
                        Image = ((ImageLinePart)Part).GetImage();
                        IsImagePage = true;
                        CurrentWritePosition = WritingDirection.GetPageEndPos();
                    }
                    else
                    {
                        AddPartIfPossible((TextLinePart)Part);
                        if (CurrentPos.CharIndex != 0)
                        {
                            while (!WritingDirection.PageFull() && CurrentPos.CharIndex != 0)
                            {
                                AddPartIfPossible((TextLinePart)Part);
                            }
                        }
                        LimitReached = WritingDirection.PageFull();
                    }
                    CurrentPos.PartIndex++;
                    if (LimitReached)
                        break;
                }
                CurrentPos.LineIndex++;
                if (LimitReached)
                    break;
                WritingDirection.Wrap();
            }
            EndPos = CurrentPos;
        }

        private void SetDirection()
        {
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
        }


        public TextLinePart AddPartIfPossible(TextLinePart Part)
        {
            if (WritingDirection.NeedsToWrap(1))
            {
                WritingDirection.Wrap();
            }
            TextLinePart DrawnPart = GetLargestFittingLinePart(Part);
            MakeTextReadyForDrawing(DrawnPart);
            return DrawnPart;
        }

        //Tested and finished
        public TextLinePart GetLargestFittingLinePart(TextLinePart Part)
        {
            if (!string.IsNullOrEmpty(Part.Ruby) || Part.Type == LinePartTypes.sesame)
            {
                if (WritingDirection.NeedsToWrap(Part.Text.Length))
                {
                    return new TextLinePart();
                }
                else
                {
                    return Part;
                }
            }

            TextLinePart Res = null;
            if (Part.Text.Length < 20)
            {
                for (int i = Part.Text.Length; i >= CurrentPos.CharIndex; i--)
                {
                    int Length = i - CurrentPos.CharIndex;
                    if (!WritingDirection.NeedsToWrap(Length))
                    {
                        Res = GetCutLinePart(Part, CurrentPos.CharIndex, Length);
                        break;
                    }
                }
            }
            else
            {
                bool LastFit = false;
                bool Fit = false;
                double LastPos = -1;
                int LastLength = -1;
                for (int i = 0; i < 100; i++)
                {
                    double Pos = GetQuickFindPos(i, CurrentPos.CharIndex, Part.Text.Length, Fit, LastPos);
                    int Length = (int)Math.Floor(Pos) - CurrentPos.CharIndex + 1;
                    Fit = !WritingDirection.NeedsToWrap(Length);

                    bool Passed = !Fit && LastFit && LastLength + 1 == Length;
                    if (Passed)
                    {
                        Length--;
                    }
                    if (Passed || (Fit && (i == 0 || Length == LastLength)) )
                    {                       
                        Res = GetCutLinePart(Part, CurrentPos.CharIndex, Length);
                        break;
                    }
                    LastPos = Pos;
                    LastFit = Fit;
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
                CurrentPos.CharIndex = 0;
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

        private void MakeTextReadyForDrawing(TextLinePart Part)
        {
            Point MainWritingPosOffset = WritingDirection.GetMainWritingOffset();
            Point UpperWritingPosOffset = WritingDirection.GetUpperWritingOffset();
            MainWritingPosOffset.Offset(CurrentWritePosition.X, CurrentWritePosition.Y);
            UpperWritingPosOffset.Offset(CurrentWritePosition.X, CurrentWritePosition.Y);

            //   if (Part.Ruby == "")
            {
                var MainFormattedText = WritingDirection.GetFormattedText(Part.Text, FontSize);
                var UpperFormatedText = WritingDirection.GetFormattedText(Part.Ruby, FontSize * RubyFontSize);

                TextParts.Add(MainFormattedText);
                TextPartPositions.Add(MainWritingPosOffset);

                TextParts.Add(UpperFormatedText);
                TextPartPositions.Add(UpperWritingPosOffset);

                Point FinalOffset = WritingDirection.GetTotalOffset(MainFormattedText);
                CurrentWritePosition.Offset(FinalOffset.X, FinalOffset.Y);
            }

            //    string Text = "";
            //add handling of special characters that need custom line spacing
            //extract all thing related to writing direction in new class

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
    }
}
