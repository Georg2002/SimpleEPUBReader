using EPUBReader;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public static class HTMLParser
    {
        public static HtmlDocument Parse(TextFile File)
        {
                Logger.Report(string.Format("Parsing html of file {0}", File.Name));
            var Doc = new HtmlDocument();
            try
            {
                Doc.LoadHtml(File.Text);
                foreach (var Error in Doc.ParseErrors)
                {
                    Logger.Report(string.Format("Error at line {0}: {1}", Error.Line, Error.Reason));
                }
            }
            catch (Exception ex)
            {
                Logger.Report("Parsing failed");
                Logger.Report(ex);
            }
            return Doc;
        }
    }
}
