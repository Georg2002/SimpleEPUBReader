namespace EPUBRenderer3
{
    internal class MarkerLetter : Letter
    {
        public string Id;
        public MarkerLetter(string Id)
        {
            this.Id = Id;
            Type = LetterTypes.Marker;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            (StartPosition, EndPosition) = GetNeutralStartingPosition(Info);
            NextWritePos = StartPosition;
            return true;
        }
    }
}
