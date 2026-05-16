using System.Drawing.Printing;
using System.Windows;

namespace EPUBRenderer
{
    internal class MarkerLetter : Letter
    {
        public string Id;
        public MarkerLetter(string Id, WordInfo wordInfo) : base(wordInfo)
        {
            this.Id = Id;
            Type = LetterTypes.Marker;
            this.IsWordEnd = true;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            this.StartPosition = this.IsPageStart ? new Vector(Info.PageSize.X - this.LineDist, 0) : PrevLetter.NextWritePos;
            this.EndPosition = this.StartPosition;
            this.NextWritePos = this.StartPosition;     
            return true;
        }
    }
}
