using System.Windows;

namespace EPUBRenderer
{
    internal class BreakLetter : Letter
    {
        private readonly double breakWidth = 1;
        public BreakLetter(WordInfo wordInfo, double breakWidth) : base(wordInfo)
        {
            Type = LetterTypes.Break;
            IsWordEnd = true;
            this.breakWidth = Math.Max(breakWidth, 1);
        }

        public override bool Position(LetterPlacementInfo info)
        {
            var PageSize = info.PageSize;

            if (this.IsPageStart)
            {
                StartPosition = new Vector(PageSize.X - this.LineDist, 0);
                EndPosition = new Vector(PageSize.X - this.LineDist - StandardFontSize, 0);
            }
            else
            {
                StartPosition = this.PrevLetter.NextWritePos;
                EndPosition = StartPosition + new Vector(-StandardFontSize, 0);
            }
            if (!IsPageStart && this.PrevLetter.Type == LetterTypes.Image)
            {
                //ignores the first break after an image in order to remove redundant breaks
                NextWritePos = StartPosition;
            }
            else NextWritePos = new Vector(this.StartPosition.X - this.GetNewLineDist() * this.breakWidth, 0);
            return true;
        }
    }
}
