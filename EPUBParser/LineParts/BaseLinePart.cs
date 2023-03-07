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
        public bool Splittable = true;
        public bool IsRuby = false;

        public BaseLinePart(LineSplitInfo info)
        {
            ActiveClasses = new string[info.ActiveClasses.Count];
            info.ActiveClasses.CopyTo(ActiveClasses);
            Splittable = info.Splittable;
            IsRuby = info.IsRuby;
        }
        public override string ToString()
        {
            return Text;
        }
    }

    public enum LinePartTypes
    {
        normal, image, paragraph, marker
    }
}
