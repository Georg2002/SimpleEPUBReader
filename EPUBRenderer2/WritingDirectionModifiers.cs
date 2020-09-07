using EPUBParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
{
    public static class WritingDirectionModifiers
    {
        private static Direction Dir;

        public static void SetDirection(EpubSettings Settings)
        {
            int Index = 0;
            if (Settings.Vertical)
            {
                Index += 2;
            }
            if (Settings.RTL)
            {
                Index += 1;
            }
            Dir = (Direction)Index;
        }

        internal static List<TextElement> GetTextInPage(RenderPage page, Vector pageSize)
        {
            int StartIndex = 0;
            int EndIndex = 0;
            int ExtraStart = 0;
            int ExtraEnd = 0;
            Vector StartPos = pageSize + page.SinglePageOffset * (page.CurrentPage - 1);
            Vector EndPos = pageSize + page.SinglePageOffset * page.CurrentPage;
            switch (Dir)
            {
                case Direction.VRTL:
                    StartIndex = page.TextElements.FindIndex(a =>
                    {
                        var First = a.First();
                        return  First.EndPos.X < StartPos.X;
                    });
                    EndIndex = page.TextElements.FindLastIndex(a =>
                    {
                        var Last = a.Last();
                        return Last.StartPos.X + 0.001>= EndPos.X && Last.ElementType 
                        != TextElementType.RubyLetter;
                    });                    
                    if (StartIndex != 0)
                    {
                        var WordBefore = page.TextElements[StartIndex - 1];
                        for (int i = 0; i < WordBefore.Count; i++)
                        {
                            var Element = WordBefore[i];
                            if (Element.EndPos.X < StartPos.X)
                            {
                                ExtraStart = WordBefore.Count - i;
                                break;
                            }
                        }
                        StartIndex--;
                    }
                    else
                    {
                        ExtraStart = page.TextElements[StartIndex].Count;
                    }
                    if (EndIndex != page.TextElements.Count - 1)
                    {
                        var WordAfter = page.TextElements[EndIndex + 1];
                        for (int i = WordAfter.Count - 1; i >= 0; i--)
                        {
                            var Element = WordAfter[i];
                            if (Element.StartPos.X >= EndPos.X)
                            {
                                ExtraEnd = i + 1;
                                break;
                            }
                        }
                        EndIndex++;
                    }
                    else
                    {
                        ExtraEnd = page.TextElements[EndIndex].Count;
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
            var Result = new List<TextElement>();
            for (int i = StartIndex; i <= EndIndex; i++)
            {
                var Word = page.TextElements[i];
                if (i == StartIndex)
                {
                    Result.AddRange(Word.GetRange(Word.Count - ExtraStart, ExtraStart));
                }
                else if (i == EndIndex)
                {
                    Result.AddRange(Word.GetRange(0, ExtraEnd));
                }
                else
                {
                    Result.AddRange(Word);
                }
            }
            return Result;
        }

        internal static Vector GetNewPagePos(Vector pageSize, int endPagePos)
        {
            var Offset = GetPageOffset(pageSize);
            switch (Dir)
            {
                case Direction.VRTL:
                    var X = GetStartPosition(pageSize).X + Offset.X * endPagePos;
                    return new Vector(X, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetBreakSize()
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(GlobalSettings.NormalFontSize,0 );
                default:
                    throw new NotImplementedException();
            }
        }

        internal static int GetPagePosition(Vector startPos, Vector pageSize)
        {
            var Offset = GetPageOffset(pageSize);
            switch (Dir)
            {
                case Direction.VRTL:
                    return (int)((startPos.X - pageSize.X) / Offset.X + 1);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetPageOffset(Vector pageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector( -pageSize.X + pageSize.X % GlobalSettings.LineHeight, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static int GetPageCount(RenderPage page, Vector pageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    var LastElement = page.TextElements.Last().Last();
                    return (int)Math.Ceiling(1 + LastElement.EndPos.X / page.SinglePageOffset.X);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Vector GetStartPosition(Vector PageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(PageSize.X - GlobalSettings.LineHeight, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Point GetWritingPosition(Letter letter)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Point(letter.StartPos.X + letter.FontSize / 2, letter.StartPos.Y);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetNextPosition(Vector currentWritePosition, TextElement letter)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    double YOffset = 0;
                    if (letter.ElementType == TextElementType.Letter || letter.ElementType == TextElementType.RubyLetter)
                    {
                        YOffset = ((Letter)letter).FontSize;
                    }
                    return new Vector(currentWritePosition.X - letter.Size.X + YOffset, currentWritePosition.Y + letter.Size.Y);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetRubyStartOffset()
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(GlobalSettings.RubyOffset, GlobalSettings.RubyFontSize * -0.5 - GlobalSettings.NormalFontSize * 0.05);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetMinRubyOffset(Vector offset)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    if (offset.Y < GlobalSettings.RubyFontSize)
                    {
                        return new Vector(0, GlobalSettings.RubyFontSize);
                    }
                    else
                    {
                        return offset;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        internal static bool NeedsToWrap(Vector endPos, Vector PageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return endPos.Y > PageSize.Y;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetNewLinePos(Vector currentWritePos, Vector pageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(currentWritePos.X - GlobalSettings.LineHeight, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetAfterImagePos(Vector currentWritePos, Vector pageSize, ImageInText Image)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    double X = Image.EndPos.X;
                    X -= Math.Ceiling(Image.Size.X / GlobalSettings.LineHeight + 1) * GlobalSettings.LineHeight;
                    return new Vector(X, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetImageStartPos(ImageInText Image, Vector currentWritePos, Vector pageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(currentWritePos.X - Image.Size.X, (pageSize.Y - Image.Size.Y) / 2);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    enum Direction
    {
        HLTR, HRTL, VLTR, VRTL
    }
}
