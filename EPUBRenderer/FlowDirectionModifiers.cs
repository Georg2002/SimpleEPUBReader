using EPUBParser;
using System;
using System.Windows;

namespace EPUBRenderer
{
    public static class FlowDirectionModifiers
    {
        public static WritingFlow Direction;

        internal static Vector GetAfterImagePosition(Vector currentWritePos, Vector PageSize, Vector dimensions)
        {
            Vector AfterImagePos = NewLinePosition(currentWritePos, PageSize);
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    AfterImagePos.X -= dimensions.X;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return NewLinePosition(AfterImagePos, PageSize);
        }

        internal static void SetDirection(EpubSettings pageSettings)
        {
            int EnumIndex = 0;
            if (!pageSettings.Vertical)
            {
                EnumIndex += 2;
            }
            if (!pageSettings.RTL)
            {
                EnumIndex += 1;
            }
            Direction = (WritingFlow)EnumIndex;
        }

        internal static Vector GetStartWritingPosition(Vector PageSize)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return new Vector(PageSize.X - ChapterPagesCreator.FontSize * ChapterPagesCreator.LineSpace, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static bool InPage(Vector Position, Vector PageSize)
        {
            return Position.X >= 0 && Position.Y >= 0 && Position.X <= PageSize.X && Position.Y <= PageSize.Y;
        }

        internal static bool NeedsToWrap(Vector Position, Vector PageSize)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return Position.Y >= PageSize.Y;
                case WritingFlow.VLTR:
                    return Position.Y >= PageSize.Y;
                case WritingFlow.HRTL:
                    return Position.X <= 0;
                case WritingFlow.HLTR:
                    return Position.X >= PageSize.X;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetAfterWritingPosition(Vector writingPos, Writing newItem)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return new Vector(writingPos.X, writingPos.Y + newItem.FontSize);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector NewLinePosition(Vector WritingPosition, Vector PageSize)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    WritingPosition.Y = ChapterPagesCreator.FontSize;
                    WritingPosition.X -= ChapterPagesCreator.FontSize * ChapterPagesCreator.LineSpace;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return WritingPosition;
        }

        internal static Vector GetRubyStartOffset()
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return new Vector(ChapterPagesCreator.FontSize * ChapterPagesCreator.RubyOffSet,
                      ChapterPagesCreator.FontSize * ChapterPagesCreator.RubyFontSize * 0.7);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector SetRenderPos(Writing Writing)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    Vector Offset = new Vector();
                    if (GlobalSettings.VerticalVisualFixes.ContainsKey(Writing.Text))
                    {
                        var Info = GlobalSettings.VerticalVisualFixes[Writing.Text];
                        Offset = Info.RenderOffset * Writing.FontSize;
                    }
                    return Offset + new Vector(Writing.FontSize / 2 + Writing.WritingPosition.X, Writing.WritingPosition.Y - Writing.FontSize);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetMarkingOffset()
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return new Vector(0, ChapterPagesCreator.FontSize * 0.2);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector GetMarkingSize()
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return new Vector(ChapterPagesCreator.FontSize, ChapterPagesCreator.FontSize);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Rect GetImageRect(Vector basePosition, Vector pageSize, Vector Dimensions)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    return new Rect(basePosition.X - Dimensions.X,
                      (basePosition.Y + pageSize.Y + Dimensions.Y) / 2, Dimensions.X, Dimensions.Y);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static Vector RubyMinimumDistance(Vector offset)
        {
            switch (Direction)
            {
                case WritingFlow.VRTL:
                    if (offset.Y < ChapterPagesCreator.RubyFontSize * ChapterPagesCreator.FontSize)
                    {
                        return new Vector(0, ChapterPagesCreator.RubyFontSize * ChapterPagesCreator.FontSize);
                    }
                    else
                    {
                        return offset;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum WritingFlow
    {
        VRTL, VLTR, HRTL, HLTR
    }
}
