using System.Windows;

namespace EPUBRenderer
{
    internal class BreakLetter : Letter
    {
        public BreakLetter(WordInfo wordInfo) : base(wordInfo)
        {
            Type = LetterTypes.Break;
            IsWordEnd = true;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PageSize = Info.PageSize;

            if (IsPageStart)
            {
                StartPosition = new Vector(PageSize.X - LineDist + StandardFontSize, 0);
                EndPosition = new Vector(PageSize.X - LineDist, 0);
            }
            else
            {
                StartPosition = PrevLetter.NextWritePos;
                EndPosition = StartPosition + new Vector(-StandardFontSize, 0);
            }
            if (!IsPageStart && PrevLetter.Type == LetterTypes.Image)
            {
                //ignores the first break after an image in order to remove redundant breaks
                NextWritePos = StartPosition;
            }
            else NextWritePos = new Vector(StartPosition.X - GetNewLineDist(), 0);          
            return true;
        }

        public override object GetRenderElement()
        {
            return null;
        }
    }
}
