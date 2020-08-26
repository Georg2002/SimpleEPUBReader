using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EPUBRenderer
{
   public static class WordSplitter
    {
        public static List<EpubLine> SplitIntoWords(EpubPage page)
        {
            var Result = new List<EpubLine>();
            foreach (var Line in page.Lines)
            {
                var CurrentLine = new EpubLine();
                Result.Add(CurrentLine);
                foreach (var Part in Line.Parts)
                {
                    if (Part.Type == LinePartTypes.normal)
                    {
                        var TextPart = (TextLinePart)Part;
                        if (string.IsNullOrEmpty(TextPart.Ruby) && !string.IsNullOrEmpty(TextPart.Text))
                        {
                            string Text = TextPart.Text;
                            int StartIndex = 0;
                            int Length = 0;
                            for (int i = 0; i < Text.Length; i++)
                            {
                                Length++;
                                char c = Text[i];
                                if (GlobalSettings.PossibleLineBreaks.Contains(c))
                                {
                                    bool Add = true;
                                    if (i != Text.Length - 1)
                                    {
                                        char n = Text[i + 1];
                                        Add = !GlobalSettings.PossibleLineBreaks.Contains(n);                                        
                                    }
                                    if (Add)
                                    {
                                        CurrentLine.Parts.Add(new TextLinePart(Text.Substring(StartIndex, Length), ""));
                                        Length = 0;
                                        StartIndex = i + 1;
                                    }                               
                                }
                            }
                            if (StartIndex != Text.Length)
                            {
                                CurrentLine.Parts.Add(new TextLinePart(Text.Substring(StartIndex), ""));
                            }
                        }
                        else
                        {
                            CurrentLine.Parts.Add(TextPart);
                        }
                    }
                    else
                    {
                        CurrentLine.Parts.Add(Part);
                    }
                }
            }
            return Result;
        }
    }
}
