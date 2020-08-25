using EPUBParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer2
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

        internal static List<List<TextElement>> GetWordsInPage(List<List<TextElement>> words, double offset, Vector pageSize)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    double MaxPos = offset + pageSize.X;
                    return words.Where(a => a.First().StartPos.X >= offset && a.Last().EndPos.X <= MaxPos).ToList();
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
                    return new Point(letter.StartPos.X + letter.FontSize /2, letter.StartPos.Y);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetNextPosition(Vector currentWritePosition, TextElement letter)
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(currentWritePosition.X, currentWritePosition.Y + letter.Size.Y);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetRubyStartOffset()
        {
            switch (Dir)
            {
                case Direction.VRTL:
                    return new Vector(GlobalSettings.RubyOffset,0);
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
