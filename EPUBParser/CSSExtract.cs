using System;
using System.Collections.Generic;
using System.Text;
using ExCSS;

namespace EPUBParser
{
    public class CSSExtract
    {
        public List<CSSStyle> Styles;
        public CSSExtract()
        {
            Styles = new List<CSSStyle>();
        }

        public void AddRules(ZipEntry file)
        {
            if (file == null)
            {
                Logger.Report("css file null", LogType.Error);
                return;
            }
            string CSS = Encoding.UTF8.GetString(file.Content);
            var Parser = new StylesheetParser();
            var Stylesheet = Parser.Parse(CSS);
            foreach (IRule Child in Stylesheet.Children)
            {
                if (Child.Type != RuleType.Style) continue;
                bool Set = false;
                var Style = (IStyleRule)Child;
                CSSStyle NewStyle = new CSSStyle();
                NewStyle.SelectorText = Style.SelectorText.Remove(0,1);//removes leading .
                string FontString = Style.Style.FontSize;
                if (!string.IsNullOrEmpty(FontString))
                {
                    bool ContainsDigit = false;
                    foreach (var c in FontString)
                    {
                        if (char.IsDigit(c)) 
                        {
                            ContainsDigit = true;
                            break;
                        }
                    }
                    if (ContainsDigit)
                    {
                        FontString = FontString.Replace("em", "");
                        FontString = FontString.Replace("%", "");
                        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                        {
                            FontString = FontString.Replace(".", ",");
                        }

                        NewStyle.FontSize = Convert.ToSingle(FontString);
                    }
                    else
                    {
                        switch (FontString)
                        {
                            case "small":
                                NewStyle.FontSize = 0.8f;
                                break;
                        }
                    }
                    Set = true;
                }
                if (!string.IsNullOrEmpty(Style.Style.FontWeight))
                {
                    Set = true;
                    switch (Style.Style.FontWeight)
                    {
                        default:
                        case "normal":
                            NewStyle.FontWeight = FontWeights.normal;
                            break;
                        case "bold":
                            NewStyle.FontWeight = FontWeights.bold;
                            break;
                        case "bolder":
                            NewStyle.FontWeight = FontWeights.bolder;
                            break;
                        case "lighter":
                            NewStyle.FontWeight = FontWeights.lighter;
                            break;
                    }
                }

                if (Set && NewStyle.NotStandard()) Styles.Add(NewStyle);              
            }
        }

    }

    public class CSSStyle
    {
        public string SelectorText;
        public float FontSize;
        public FontWeights FontWeight;        
        public CSSStyle()
        {
            FontSize = 1;
            FontWeight = FontWeights.normal;
        }

        public override string ToString()
        {
            return SelectorText;
        }

        internal bool NotStandard()
        {
            return FontSize != 1 || FontWeight != FontWeights.normal;
        }
    }

    public enum FontWeights
    {
        normal,bold,bolder,lighter
    }
}