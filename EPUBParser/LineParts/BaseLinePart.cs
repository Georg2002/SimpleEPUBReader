using System.Collections.Generic;

namespace EPUBParser
{
    public class BaseLinePart
    {
        public string Text;
        public LinePartTypes Type;
        public string[] ActiveClasses;

        public override string ToString()
        {
            return Text;
        }
    }

    public enum LinePartTypes
    {
        normal, sesame, image, paragraph, marker
    }
}
