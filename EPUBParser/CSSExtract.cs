using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExCSS;

namespace EPUBParser
{
    public class CSSExtract
    {
        const double defaultFontSize = 16;//16 as default browser font size
        public List<CSSStyle> Styles = new List<CSSStyle>
            {
                //headers
                new CSSStyle() { FontSize = 2f, FontWeight = FontWeights.bold, SelectorText = "h1" },
                new CSSStyle() { FontSize = 1.5f, FontWeight = FontWeights.bold, SelectorText = "h2" },
                new CSSStyle() { FontSize = 1.17f, FontWeight = FontWeights.bold, SelectorText = "h3" },
                new CSSStyle() { FontSize = 1f, FontWeight = FontWeights.bold, SelectorText = "h4" },
                new CSSStyle() { FontSize = 0.83f, FontWeight = FontWeights.bold, SelectorText = "h5" },
                new CSSStyle() { FontSize = 0.67f, FontWeight = FontWeights.bold, SelectorText = "h6" }
            };
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
                var Style = (IStyleRule)Child;
                CSSStyle NewStyle = new CSSStyle();
                NewStyle.SelectorText = Style.SelectorText.Remove(0, 1);//removes leading .
                string FontString = Style.Style.FontSize;
                if (!string.IsNullOrEmpty(FontString))
                {
                    bool ContainsDigit = FontString.Any(a => char.IsDigit(a));
                    if (ContainsDigit)
                    {
                        NewStyle.FontSize = (float)fromSize(FontString);
                    }
                    else
                    {
                        switch (FontString)
                        {
                            case "small":
                                NewStyle.FontSize = 0.8f;
                                break;
                            default:
                                Logger.Report("Font size specifier not found: " + FontString, LogType.Error);
                                break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(Style.Style.FontWeight))
                {
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
                string widthString = Style.Style.Width;
                string heightString = Style.Style.Height;
                if (!string.IsNullOrEmpty(widthString))
                {
                    NewStyle.Width = this.FromDimension(widthString) * NewStyle.FontSize;
                }
                if (!string.IsNullOrEmpty(heightString))
                {
                    NewStyle.Height = this.FromDimension(heightString) * NewStyle.FontSize;
                }
                if (NewStyle.NotStandard()) Styles.Add(NewStyle);
            }
        }

        private double fromSize(string sizeString)
        {
            if (sizeString.EndsWith("px"))
            {
                sizeString = sizeString.Replace("px", "");
                return Convert.ToSingle(sizeString) / defaultFontSize;
            }
            else
            {
                sizeString = sizeString.Replace("em", "");
                sizeString = sizeString.Replace("%", "");
                if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                {
                    sizeString = sizeString.Replace(".", ",");
                }

                return Convert.ToSingle(sizeString);
            }
        }
        private double? FromDimension(string dimensionString)
        {
            if (dimensionString == null || dimensionString == "auto") return null;       
            return fromSize(dimensionString);
        }
    }

    public class CSSStyle
    {
        public string SelectorText;
        public float FontSize = 1;
        public FontWeights FontWeight = FontWeights.normal;
        //for images
        public double? Width = null;
        public double? Height = null;
        public override string ToString() => SelectorText;
        internal bool NotStandard()
        {
            return FontSize != 1 || FontWeight != FontWeights.normal || Width != null || Height != null;
        }
    }

    public enum FontWeights
    {
        normal, bold, bolder, lighter
    }
}