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

    internal class Letter
    {
        public const float FontSizeStandard = 15;
        public const float RubyFontSize = 0.5f * FontSizeStandard;
        public const float LineDist = 1.2f *( FontSizeStandard + RubyFontSize);

        public Vector StartPosition;
        public Vector EndPosition;
        public Vector NextWritePos;
        public LetterTypes Type;
        public byte MarkingColorIndex;
        public virtual bool Position(Word Previous,Letter PrevLetter, Word OwnWord, Vector PageSize)
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

            if( CharInfo.SpecialCharacters.ContainsKey(Character))
            {
                var Info = CharInfo.SpecialCharacters[Character];
                Offset = Info.Offset;
                RelScale = Info.Scaling;
                this.Character = Info.Replacement;
            }
        }

        public override bool Position(Word PreviousWord, Letter PrevLetter,Word OwnWord, Vector PageSize)
        {
          //  if (OwnWord.Type == WordTypes.Normal)
            {
                if (PrevLetter == null)
                {
                    StartPosition = new Vector(PageSize.X, 0);
                    FontSize = FontSizeStandard;
                }
                else
                {
                    StartPosition = PrevLetter.NextWritePos;
                }
                EndPosition = StartPosition + new Vector(-FontSize, FontSize);
                NextWritePos = EndPosition + new Vector(FontSize, 0);
                return EndPosition.Y > PageSize.Y;
            }
         //   else
            {
                FontSize = RubyFontSize;
                return true;
            }
        }

        public override object GetRenderElement()
        {           
            return new FormattedText(Character.ToString(), System.Globalization.CultureInfo.InvariantCulture,FlowDirection.RightToLeft, StandardTypeface, FontSize, Brushes.Black,1);
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

        public override bool Position(Word Previous, Letter PrevLetter, Word OwnWord, Vector PageSize)
        {
            if (PrevLetter==null)
            {
                StartPosition = new Vector(PageSize.X, 0);               
            }
            else
            {
                StartPosition = new Vector(PrevLetter.EndPosition.X, 0);
            }
            EndPosition = StartPosition + new Vector(-Image.Width, Image.Height);
            NextWritePos = new Vector(EndPosition.X, 0);
            return EndPosition.Y > PageSize.Y;
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

        public override bool Position(Word Previous, Letter PrevLetter, Word OwnWord, Vector PageSize)
        {
            StartPosition = PrevLetter.EndPosition + new Vector(-FontSizeStandard, 0);
            EndPosition = PrevLetter.EndPosition;
            NextWritePos = new Vector(StartPosition.X - LineDist, 0);
            return true;
        }

        public override object GetRenderElement()
        {
            return null;
        }
    }
}
