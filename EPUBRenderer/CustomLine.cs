using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace EPUBRenderer
{
    internal class CustomLine : TextLine
    {
        public override double Baseline => throw new NotImplementedException();

        public override int DependentLength => throw new NotImplementedException();

        public override double Extent => throw new NotImplementedException();

        public override bool HasCollapsed => throw new NotImplementedException();

        public override bool HasOverflowed => throw new NotImplementedException();

        public override double Height => throw new NotImplementedException();

        public override int Length => throw new NotImplementedException();

        public override double MarkerBaseline => throw new NotImplementedException();

        public override double MarkerHeight => throw new NotImplementedException();

        public override int NewlineLength => throw new NotImplementedException();

        public override double OverhangAfter => throw new NotImplementedException();

        public override double OverhangLeading => throw new NotImplementedException();

        public override double OverhangTrailing => throw new NotImplementedException();

        public override double Start => throw new NotImplementedException();

        public override double TextBaseline => throw new NotImplementedException();

        public override double TextHeight => throw new NotImplementedException();

        public override int TrailingWhitespaceLength => throw new NotImplementedException();

        public override double Width => throw new NotImplementedException();

        public override double WidthIncludingTrailingWhitespace => throw new NotImplementedException();

        public override TextLine Collapse(params TextCollapsingProperties[] collapsingPropertiesList)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Draw(DrawingContext drawingContext, Point origin, InvertAxes inversion)
        {

            throw new NotImplementedException();
        }

        public override CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit)
        {
            throw new NotImplementedException();
        }

        public override CharacterHit GetCharacterHitFromDistance(double distance)
        {
            throw new NotImplementedException();
        }

        public override double GetDistanceFromCharacterHit(CharacterHit characterHit)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns()
        {
            throw new NotImplementedException();
        }

        public override CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
        {
            throw new NotImplementedException();
        }

        public override CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
        {
            throw new NotImplementedException();
        }

        public override IList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength)
        {
            throw new NotImplementedException();
        }

        public override IList<TextCollapsedRange> GetTextCollapsedRanges()
        {
            throw new NotImplementedException();
        }

        public override TextLineBreak GetTextLineBreak()
        {
            throw new NotImplementedException();
        }

        public override IList<TextSpan<TextRun>> GetTextRunSpans()
        {
            throw new NotImplementedException();
        }
    }
}
