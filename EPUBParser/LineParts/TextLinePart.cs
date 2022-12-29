using System.Collections.Generic;

namespace EPUBParser
{
    public class TextLinePart : BaseLinePart
    {
        public TextLinePart(string Text, LineSplitInfo info) : base(info)
        {
            this.Text = Text;
            Type = LinePartTypes.normal;
        }
    }
}
