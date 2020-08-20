using EPUBParser;
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
    public static class FlowDirectionModifiers
    {
        public static WritingFlow Direction;

        internal static Vector GetNewLinePos(Vector currentWritePos, Vector pageSize)
        {
            throw new NotImplementedException();
        }

        internal static Vector GetAfterImagePosition(Vector currentWritePos, Vector pageSize, Vector dimensions)
        {
            throw new NotImplementedException();
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

        internal static Vector GetRenderPos(Vector writingPosition)
        {
            throw new NotImplementedException();
        }

        internal static Vector GetAfterWritingPosition(Vector writingPosCopy, Writing newItem)
        {
            throw new NotImplementedException();
        }

        internal static void WrapIntoPage(Writing writing)
        {
            throw new NotImplementedException();
        }
    }

    public enum WritingFlow
    {
        VRTL, VLTR, HRTL, HLTR
    }
}
