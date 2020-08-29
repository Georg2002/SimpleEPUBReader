using EPUBParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
{
    public static class TextPositioner
    {
        private static Vector PageSize;
        private static Vector CurrentWritePos;

        public static void Position(List<List<TextElement>> ElementString, Vector PageSize, EpubSettings PageSettings)
        {
            WritingDirectionModifiers.SetDirection(PageSettings);
            TextPositioner.PageSize = PageSize;
            CurrentWritePos = WritingDirectionModifiers.GetStartPosition(PageSize);

            for (int WordIndex = 0; WordIndex < ElementString.Count; WordIndex++)
            {
                var CurrentWord = ElementString[WordIndex];
                if (CurrentWord.Count == 0) continue;
                var First = CurrentWord.First();
                switch (First.ElementType)
                {
                    case TextElementType.Letter:
                        foreach (var Letter in CurrentWord)
                        {
                            Letter.StartPos = CurrentWritePos;
                            CurrentWritePos = WritingDirectionModifiers.GetNextPosition(CurrentWritePos, Letter);
                        }
                        WrapIntoPage(CurrentWord);
                        break;
                    case TextElementType.RubyLetter:
                        var MainWord = ElementString[WordIndex - 1];                      
                        var StartLetter = MainWord.First();
                        var EndLetter = MainWord.Last();
                        Vector WordLength = WritingDirectionModifiers.GetNextPosition(EndLetter.StartPos, EndLetter) - StartLetter.StartPos;
                        Vector Offset = WordLength / CurrentWord.Count;
                        Offset = WritingDirectionModifiers.GetMinRubyOffset(Offset);
                        Vector StartWritePos = StartLetter.StartPos + (WordLength - Offset * (CurrentWord.Count - 1)) / 2 + WritingDirectionModifiers.GetRubyStartOffset();
                        for (int i = 0; i < CurrentWord.Count; i++)
                        {
                            CurrentWord[i].StartPos = StartWritePos;
                            StartWritePos += Offset;
                        }
                        break;
                    case TextElementType.Image:
                        ImageInText Image = (ImageInText)First;
                        if (Image.Size.X > PageSize.X || Image.Size.Y > PageSize.Y || ElementString.Count == 1)
                        {
                            double WidthRatio = Image.Size.X / PageSize.X;
                            double HeightRatio = Image.Size.Y / PageSize.Y;
                            double EffectiveRatio = Math.Max(WidthRatio, HeightRatio);
                            Image.Size /= EffectiveRatio;
                        }
                        if (ElementString.Count > 1 )
                        {
                            CurrentWritePos = WritingDirectionModifiers.GetNewLinePos(CurrentWritePos, PageSize);
                            Image.StartPos = WritingDirectionModifiers.GetImageStartPos(Image, CurrentWritePos, PageSize);
                            CurrentWritePos = WritingDirectionModifiers.GetAfterImagePos(CurrentWritePos, PageSize, Image);
                        }
                        else
                        {                           
                            Image.StartPos = (PageSize - Image.Size) / 2;
                            CurrentWritePos = WritingDirectionModifiers.GetNextPosition(CurrentWritePos, First);
                            CurrentWritePos = WritingDirectionModifiers.GetNewLinePos(CurrentWritePos, PageSize);
                        }
                        break;
                    case TextElementType.Break:
                        CurrentWritePos = WritingDirectionModifiers.GetNewLinePos(CurrentWritePos, PageSize);
                        break;
                }
            }
        }

        private static void WrapIntoPage(List<TextElement> currentWord)
        {
            if (WritingDirectionModifiers.NeedsToWrap(currentWord.Last().EndPos, PageSize))
            {
                Vector NewLinePos = WritingDirectionModifiers.GetNewLinePos(CurrentWritePos, PageSize);
                var ReferenceOffset = NewLinePos - currentWord.First().StartPos;
                if (WritingDirectionModifiers.NeedsToWrap(currentWord.Last().EndPos + ReferenceOffset, PageSize))
                {
                    ReferenceOffset = new Vector();
                    foreach (var Letter in currentWord)
                    {
                        var NewStartPos = Letter.StartPos + ReferenceOffset;
                        if (WritingDirectionModifiers.NeedsToWrap(Letter.EndPos + ReferenceOffset, PageSize))
                        {
                            NewLinePos = WritingDirectionModifiers.GetNewLinePos(NewStartPos, PageSize);
                            ReferenceOffset = NewLinePos - Letter.StartPos;
                            NewStartPos = Letter.StartPos + ReferenceOffset;
                        }
                        Letter.StartPos = NewStartPos;
                    }
                }
                else
                {
                    foreach (var Letter in currentWord)
                    {
                        Letter.StartPos += ReferenceOffset;
                    }
                }

                CurrentWritePos = WritingDirectionModifiers.GetNextPosition(currentWord.Last().StartPos, currentWord.Last());
            }
        }
    }
}
