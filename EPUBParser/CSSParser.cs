using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
  public static  class CSSParser
    {
        private static readonly StylesheetParser Parser  = new StylesheetParser();

        public static Stylesheet ParseCSS(TextFile File)
        {
            var Stylesheet = Parser.Parse(File.Text);
            return Stylesheet;
        }
    }
}
