using System.Collections.Generic;

namespace EPUBParser
{
    public class BreakLinePart : BaseLinePart
    {
        public BreakLinePart(List<string> activeClasses) : base(activeClasses)
        {
            Type = LinePartTypes.paragraph;
        }
    } 
}
