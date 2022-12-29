using System.Collections.Generic;

namespace EPUBParser
{
    public class TextLinePart : BaseLinePart
    {
        public string Ruby;        

        public TextLinePart(string Text, string Ruby, List<string> ActiveClasses) : base(ActiveClasses)
        {        
            this.Text = Text;
            this.Ruby = Ruby;
            Type = LinePartTypes.normal;
        }

        public TextLinePart() : base(new List<string>())
        {
            Type = LinePartTypes.normal;
            Text = "";
            Ruby = "";
        }
    }
}
