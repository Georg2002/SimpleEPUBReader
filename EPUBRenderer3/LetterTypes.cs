using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
    internal enum LetterTypes
    {
        Letter, Image, Break
    }

    internal struct LetterPlacementInfo
    {
        public Word PrevWord;
        public Letter PrevLetter;
        public Word OwnWord;
        public Vector PageSize;
        public bool NewLine;
        public bool TightFit;
    }

    internal class Letter
    {
        public const float FontSizeStandard = 20;
        public const float RubyFontSize = 0.5f * FontSizeStandard;
        public const float LineDist = 1.2f * (FontSizeStandard + RubyFontSize);

        public Vector StartPosition;
        public Vector EndPosition;
        public Vector NextWritePos;
        public LetterTypes Type;
        public byte MarkingColorIndex;
        public virtual bool Position(LetterPlacementInfo Info)
        {
            return false;
        }

        public virtual object GetRenderElement()
        {
            return null;
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public bool InsidePage(Vector PageSize)
        {
            return EndPosition.X >= 0 && EndPosition.Y <= PageSize.Y;
        }
    }

    internal class TextLetter : Letter
    {
        public float RelScale = 1;
        public float FontSize;
        public char Character;
        public Vector Offset;
        private static Typeface StandardTypeface = new Typeface(new FontFamily("Hiragino Sans GB W3"), FontStyles.Normal,
     FontWeights.Normal, new FontStretch(), new FontFamily("MS Mincho"));

        public TextLetter(char Character)
        {
            this.Character = Character;
            Type = LetterTypes.Letter;

            if (CharInfo.SpecialCharacters.ContainsKey(Character))
            {
                var Info = CharInfo.SpecialCharacters[Character];
                Offset = Info.Offset;
                RelScale = Info.Scaling;
                this.Character = Info.Replacement;
            }
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;
            var TightFit = Info.TightFit;
            var NewLine = Info.NewLine;
            var OwnWord = Info.OwnWord;
            //  if (OwnWord.Type == WordTypes.Normal)
            {
                if (OwnWord.Type != WordTypes.Normal)
                {
                    Character = '#';
                }
                FontSize = FontSizeStandard;
                if (PrevLetter == null)
                {
                    StartPosition = new Vector(PageSize.X - LineDist + FontSize, 0);
                }
                else
                {
                    StartPosition = PrevLetter.NextWritePos;
                }
              StartPosition =  NewLine ? new Vector(StartPosition.X-LineDist,0): StartPosition;
                EndPosition = StartPosition + new Vector(-FontSize, FontSize);
                NextWritePos = EndPosition + new Vector(FontSize, 0);
            
                    if (TightFit&&EndPosition.Y > PageSize.Y)
                    {
                        StartPosition.Y = 0;
                        StartPosition.X = StartPosition.X - LineDist;
                        EndPosition = StartPosition + new Vector(-FontSize, FontSize);
                        NextWritePos = EndPosition + new Vector(FontSize, 0);
                    }
                    return InsidePage(PageSize);
              
              
            }
            //   else
            {
                FontSize = RubyFontSize;
                return true;
            }
        }

        public override object GetRenderElement()
        {
            return new FormattedText(Character.ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.RightToLeft, StandardTypeface, FontSize * RelScale, Brushes.Black, 1);
        }

        public override string ToString()
        {
            return Character.ToString();
        }
    }

    internal class ImageLetter : Letter
    {
        public ImageSource Image;

        public ImageLetter(ImageSource Image)
        {
            Type = LetterTypes.Image;
            this.Image = Image;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;

            if (PrevLetter == null)
            {
                StartPosition = new Vector(PageSize.X, 0);
            }
            else
            {
                StartPosition = new Vector(PrevLetter.EndPosition.X, 0);
            }
            if (PageSize.X < Image.Width || PageSize.Y < Image.Height)
            {
                double PRatio = PageSize.X / PageSize.Y;
                double IRatio = Image.Width / Image.Height;
                var RenderSize = PRatio < IRatio ? new Vector(-PageSize.X, PageSize.X / IRatio) : new Vector(-PageSize.Y * IRatio, PageSize.Y);
                StartPosition = PRatio < IRatio ? new Vector(StartPosition.X, (PageSize.Y - RenderSize.Y) / 2) : StartPosition;
                EndPosition = StartPosition + RenderSize;
            }
            else
            {
                EndPosition = StartPosition + new Vector(-Image.Width, Image.Height);
            }

            NextWritePos = new Vector(EndPosition.X, 0);
            return EndPosition.Y <= PageSize.Y;
        }

        public override object GetRenderElement()
        {
            return Image;
        }
    }

    internal class BreakLetter : Letter
    {
        public BreakLetter()
        {
            Type = LetterTypes.Break;
        }

        public override bool Position(LetterPlacementInfo Info)
        {
            var PrevLetter = Info.PrevLetter;
            var PageSize = Info.PageSize;

            if (PrevLetter == null)
            {
                StartPosition = new Vector(PageSize.X - LineDist + FontSizeStandard, 0);
                EndPosition = new Vector(PageSize.X - LineDist, 0);
            }
            else
            {
                StartPosition = PrevLetter.EndPosition + new Vector(-FontSizeStandard, 0);
                EndPosition = PrevLetter.EndPosition;
            }

            NextWritePos = new Vector(StartPosition.X - LineDist, 0);
            return true;
        }

        public override object GetRenderElement()
        {
            return null;
        }
    }
}
