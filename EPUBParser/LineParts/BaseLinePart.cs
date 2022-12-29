using System;
using System.Collections.Generic;
using System.Linq;

namespace EPUBParser
{
    public abstract class BaseLinePart
    {
        public string Text;
        public LinePartTypes Type;
        public string[] ActiveClasses;

        public BaseLinePart(List<string> activeClasses)
        {
            this.ActiveClasses = new string[activeClasses.Count];
            activeClasses.CopyTo(this.ActiveClasses);
        }
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
