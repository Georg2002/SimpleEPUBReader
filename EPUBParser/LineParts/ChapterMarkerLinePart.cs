using System.Collections.Generic;

namespace EPUBParser
{
    public class ChapterMarkerLinePart : BaseLinePart
    {
        public string Id;
        public ChapterMarkerLinePart(string Id, List<string> activeClasses) : base(activeClasses)
        {
            this.Id = Id;
            this.Type = LinePartTypes.marker;
        }
    }
}
