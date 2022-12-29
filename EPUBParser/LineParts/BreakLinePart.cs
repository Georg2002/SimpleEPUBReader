using System.Collections.Generic;

namespace EPUBParser
{
    public class BreakLinePart : BaseLinePart
    {
        public BreakLinePart(LineSplitInfo info) : base(info)
        {
            Type = LinePartTypes.paragraph;
        }
    } 
}
