namespace EPUBParser
{
    public class ChapterMarkerLinePart : BaseLinePart
    {
        public string Id;
        public ChapterMarkerLinePart(string Id)
        {
            this.Id = Id;
            this.Type = LinePartTypes.marker;
        }
    }
}
