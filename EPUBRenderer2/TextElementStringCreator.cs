using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer
{
    public static class TextElementStringCreator
    {
        public static List<List<TextElement>> GetElements(List<EpubLine> Lines, bool Vertical)
        {
            var Result = new List<List<TextElement>>();
            var NewWord = new List<TextElement>();
            foreach (var Line in Lines)
            {
                for (int i = 0; i < Line.Parts.Count; i++)
                {
                    var Part = Line.Parts[i];
                    if (Part.Type == LinePartTypes.image)
                    {
                        NewWord.Add(new ImageInText(((ImageLinePart)Part).GetImage()));
                        Result.Add(NewWord);
                        NewWord = new List<TextElement>();
                    }
                    else
                    {
                        var TextPart = (TextLinePart)Part;
                        if (!string.IsNullOrEmpty(TextPart.Ruby) && i != Line.Parts.Count - 1)
                        {
                            var NextPart = Line.Parts[i + 1];
                            if (NextPart.Type != LinePartTypes.image)
                            {
                                int k = i + 1;
                                bool OneMissed = false;
                                while (k != Line.Parts.Count - 1)
                                {
                                    if (NextPart.Type != LinePartTypes.image)
                                    {
                                        var NextTextPart = (TextLinePart)NextPart;
                                        if (!string.IsNullOrEmpty(NextTextPart.Ruby))
                                        {
                                            TextPart.Text += NextTextPart.Text;
                                            TextPart.Ruby += NextTextPart.Ruby;
                                            NextTextPart.Text = "";
                                            NextTextPart.Ruby = "";
                                            k++;
                                            NextPart = Line.Parts[k];
                                        }
                                        else if (!OneMissed)
                                        {
                                            OneMissed = true;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                        }

                        if (string.IsNullOrEmpty(TextPart.Text)) continue;
                        if (TextPart.Type == LinePartTypes.sesame)
                        {
                            TextPart.Ruby = new string('﹅', TextPart.Text.Length);
                        }
                        foreach (char c in TextPart.Text)
                        {
                            NewWord.Add(new Letter(GetCorrectedChar(c, Vertical), GlobalSettings.NormalFontSize));
                        }
                        Result.Add(NewWord);
                        NewWord = new List<TextElement>();
                        if (string.IsNullOrEmpty(TextPart.Ruby)) continue;
                        foreach (char c in TextPart.Ruby)
                        {
                            NewWord.Add(new RubyElement(GetCorrectedChar(c, Vertical), GlobalSettings.RubyFontSize));
                        }
                        Result.Add(NewWord);
                        NewWord = new List<TextElement>();
                    }
                }
                if (Line != Lines.Last())
                {
                    NewWord.Add(new BreakElement());
                    Result.Add(NewWord);
                    NewWord = new List<TextElement>();
                }
            }
            return Result;
        }

        private static char GetCorrectedChar(char c, bool Vertical)
        {
            if (Vertical)
            {
                if (GlobalSettings.VerticalVisualFixes.ContainsKey(c))
                {
                    return GlobalSettings.VerticalVisualFixes[c];
                }
            }
            return c;
        }
    }
}
