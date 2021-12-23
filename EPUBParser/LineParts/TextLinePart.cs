using System.Collections.Generic;

namespace EPUBParser
{
    public class TextLinePart : BaseLinePart
    {
        public string Ruby;        

        public TextLinePart(string Text, string Ruby, List<string> ActiveClasses)
        {
            this.ActiveClasses = new string[ActiveClasses.Count];
           ActiveClasses.CopyTo(this.ActiveClasses);
            this.Text = Text;
            this.Ruby = Ruby;
            Type = LinePartTypes.normal;
        }

        public TextLinePart()
        {
            Type = LinePartTypes.normal;
            Text = "";
            Ruby = "";
        }
    }
}
