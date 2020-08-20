using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
  public  class WritingDirectionModifiers
    {
        public PageRenderer2 renderer;

        public WritingDirectionModifiers(PageRenderer2 renderer)
        {
            this.renderer = renderer;
        }

        public bool PageFull()
        {
            switch (renderer.Direction)
            {
                case WritingFlow.VLTR:
                    return renderer.CurrentWritePosition.X + PageRenderer.FontSize * PageRenderer.LineSpace > renderer.PageWidth;
                case WritingFlow.VRTL:
                    return renderer.CurrentWritePosition.X - PageRenderer.FontSize * PageRenderer.LineSpace < 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool NeedsToWrap(int NewCharacters)
        {
            switch (renderer.Direction)
            {
                default:
                    throw new NotImplementedException();
                //    case WritingFlow.HLTR:
                //        return CurrentWritePosition.X + FontSize * (NewCharacters - 0.5) > PageWidth;
                //    case WritingFlow.HRTL:
                //        return CurrentWritePosition.X - FontSize * (NewCharacters - 0.5) < 0;
                case WritingFlow.VLTR:
                    return renderer.CurrentWritePosition.Y + PageRenderer.FontSize * NewCharacters > renderer.PageHeight;
                case WritingFlow.VRTL:
                    return renderer.CurrentWritePosition.Y + PageRenderer.FontSize * NewCharacters > renderer.PageHeight;
            }
        }

        //Tested and unfinished
        public void Wrap()
        {
            switch (renderer.Direction)
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
                    renderer.CurrentWritePosition.X = renderer.CurrentWritePosition.X + PageRenderer.FontSize * PageRenderer.LineSpace;
                    renderer.CurrentWritePosition.Y = 0;
                    break;
                case WritingFlow.VRTL:
                    renderer.CurrentWritePosition.X = renderer.CurrentWritePosition.X - PageRenderer.FontSize * PageRenderer.LineSpace;
                    renderer.CurrentWritePosition.Y = 0;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public FormattedText GetFormattedText(string Text, double FontSize)
        {
            FlowDirection flowDirection;
            string DrawnText = "";
            if (renderer.Page.PageSettings.Vertical)
            {
                flowDirection = FlowDirection.LeftToRight;
                foreach (char c in Text)
                {                    
                    if (GlobalSettings.VerticalVisualFixes.ContainsKey(c))
                    {
                        DrawnText += GlobalSettings.VerticalVisualFixes[c].Replacement + "\n";
                    }
                    else
                    {
                        DrawnText += c + "\n";
                    }
                }
            }
            else
            {
                if (renderer.Page.PageSettings.RTL)
                {
                    flowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    flowDirection = FlowDirection.LeftToRight;
                }
                DrawnText = Text;
            }
            var res = new FormattedText(DrawnText, CultureInfo.InvariantCulture,
                flowDirection, PageRenderer.Typeface, FontSize, Brushes.Black, 1);
            if (renderer.Page.PageSettings.Vertical)
            {
                res.LineHeight = PageRenderer.FontSize;
            }
            else
            {
                res.LineHeight = PageRenderer.FontSize * PageRenderer.LineSpace;
            }

            return res;
        }

        public Point GetPageStartPos()
        {

            switch (renderer.Direction)
            {
                case WritingFlow.VRTL:
                    return new Point(renderer.PageWidth - PageRenderer.FontSize * PageRenderer.LineSpace, 0);
                case WritingFlow.VLTR:
                    return new Point(PageRenderer.FontSize * PageRenderer.LineSpace, 0); ;
                default:
                    throw new NotImplementedException();
            }
        }

        public Point GetPageEndPos()
        {
            switch (renderer.Direction)
            {
                case WritingFlow.VRTL:
                    return new Point(-1, 0);
                case WritingFlow.VLTR:
                    return new Point(renderer.PageWidth + 1, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        public Point GetMainWritingOffset()
        {
            switch (renderer.Direction)
            {
                case WritingFlow.VRTL:
                    return new Point(PageRenderer.FontSize / 2, 0);
                case WritingFlow.VLTR:
                    return new Point(-PageRenderer.FontSize / 2, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        public Point GetUpperWritingOffset()
        {
            switch (renderer.Direction)
            {
                case WritingFlow.VRTL:
                    return new Point(PageRenderer.FontSize * (PageRenderer.LineSpace - 0.5), 0);
                case WritingFlow.VLTR:
                    return new Point(-PageRenderer.FontSize * (PageRenderer.LineSpace - 0.5), 0);
                default:
                    throw new NotImplementedException();
            }
        }

        public Point GetTotalOffset(FormattedText mainFormattedText)
        {
            switch (renderer.Direction)
            {
                case WritingFlow.VRTL:
                    return new Point(0, mainFormattedText.Height);
                case WritingFlow.VLTR:
                    return new Point(0, mainFormattedText.Height);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
