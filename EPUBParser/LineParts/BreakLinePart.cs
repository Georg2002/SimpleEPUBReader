using System.Collections.Generic;

namespace EPUBParser
{
    public class BreakLinePart : BaseLinePart
    {
        public double BreakWidth { get; private set; }
        public BreakLinePart(LineSplitInfo info, double breakWidth = 1) : base(info)
        {
            Type = LinePartTypes.paragraph;
            this.BreakWidth = breakWidth;
        }
    }
}
