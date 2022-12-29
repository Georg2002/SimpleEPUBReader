using System.Collections.Generic;

namespace EPUBParser
{
    public class ChapterMarkerLinePart : BaseLinePart
    {
        public string Id;
        public ChapterMarkerLinePart(string Id, LineSplitInfo info) : base(info)
        {
            this.Id = Id;
            this.Type = LinePartTypes.marker;
        }
    }
}
