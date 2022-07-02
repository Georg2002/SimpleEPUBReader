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
        public static HtmlDocument Parse(ZipEntry File)
        {
            var Doc = new HtmlDocument
            {
                OptionAutoCloseOnEnd = false,
                OptionOutputAsXml = true,
                OptionFixNestedTags = true
            };
            if (File == null)
            {
                Logger.Report("file is null, can't parse HTML", LogType.Error);
                return Doc;
            }

            Logger.Report(string.Format("Parsing html of file \"{0}\"", File.Name), LogType.Info); 

            try
            {
                if (File.Content == null) File.Content = new byte[0];             

                Doc.LoadHtml(Encoding.UTF8.GetString(File.Content));
                foreach (var Error in Doc.ParseErrors)
                {
                    Logger.Report(string.Format("Error at line {0}: {1}", Error.Line, Error.Reason), LogType.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Report("Parsing failed", LogType.Error);
                Logger.Report(ex);
            }
            return Doc;
        }

        internal static HtmlNode SafeNodeGet(HtmlNode ParentNode, string NodeName)
        {
            if (ParentNode == null)
            {
                Logger.Report(string.Format("Parent node is null, can't get node \"{0}\""
                    , NodeName), LogType.Error);
                return null;
            }
            var Node = ParentNode.Element(NodeName);            
            if (Node == null)
            {
                Logger.Report(string.Format("Node \"{0}\" couldn't be found"
                    , NodeName), LogType.Error);
                return null;
            } 
            else return Node;
        }

        internal static string SafeNodeTextGet(HtmlNode ParentNode, string NodeName) => SafeNodeGet(ParentNode, NodeName)?.InnerText ?? "";      

        internal static string SafeAttributeGet(HtmlNode Node, string AttributeName, bool IgnoreMissing = false)
        {
            if (Node == null)
            {
                Logger.Report(string.Format("no node passed to find attribute \"{0}\"", AttributeName), LogType.Error);
                return "";
            }
            var Attribute = Node.Attributes.FirstOrDefault(a => a.Name == AttributeName);
            if (Attribute == null)
            {
                if (!IgnoreMissing)
                {
                    Logger.Report(string.Format("attribute \"{0}\" not found", AttributeName), LogType.Error);
                }
                return "";
            }
            else return Attribute.Value;           
        }
    }
}
