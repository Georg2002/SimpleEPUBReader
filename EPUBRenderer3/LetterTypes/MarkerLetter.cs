namespace EPUBRenderer3
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
            (StartPosition, EndPosition) = GetNeutralStartingPosition(Info);
            NextWritePos = StartPosition;
            return true;
        }
    }
}
