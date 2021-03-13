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
        public Vector StartPosition;
        public Vector EndPosition;
        public Vector Offset;
        public LetterTypes Type;
        public byte MarkingColorIndex;
        public virtual bool Position(Word Previous, Vector PageSize)
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
        public char Character;

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

        public override bool Position(Word Previous, Vector PageSize)
        {
          return  base.Position(Previous, PageSize);
        }

        public override object GetRenderElement()
        {
            return base.GetRenderElement();
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

        public override bool Position(Word Previous, Vector PageSize)
        {
            return base.Position(Previous, PageSize);
        }

        public override object GetRenderElement()
        {
            return base.GetRenderElement();
        }
    }

    internal class BreakLetter : Letter
    {
        public BreakLetter()
        {
            Type = LetterTypes.Break;
        }

        public override bool Position(Word Previous, Vector PageSize)
        {
            return base.Position(Previous, PageSize);
        }

        public override object GetRenderElement()
        {
            return base.GetRenderElement();
        }
    }
}
