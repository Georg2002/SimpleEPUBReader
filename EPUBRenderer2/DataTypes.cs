using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer2
{
    public class Chapter
    {
        public List<TextElement> TextElements;
        public double PageJumpSize;
    }

    public class Letter : TextElement
    {
        private char _Text;
        public char Text
        {
            get => _Text;
            set { _Text = value; SetSize(); }
        }
        public double FontSize;
        public FormattedText FormattedText
        {
            get
            {
                return new FormattedText(Text.ToString(), CultureInfo.InvariantCulture,
                    GlobalSettings.NormalFlowDirection, GlobalSettings.NormalTypeface,
                    FontSize, GlobalSettings.NormalFontColor, 1)
                { LineHeight = FontSize };
            }
        }
        public override void SetSize()
        {
            var SizeTemplate = FormattedText;
            Size.X = SizeTemplate.Width;
            Size.Y = SizeTemplate.Height;
        }
        public Letter(char Letter, double FontSize)
        {
            ElementType = TextElementType.Letter;
            this.FontSize = FontSize;
            this.Text = Letter;
        }
    }

    public class RubyElement : Letter
    {
        //a ruby word always belongs to the normal word in front of it
        public RubyElement(char Letter, double FontSize) : base (Letter, FontSize)
        {
            ElementType = TextElementType.RubyLetter;         
        }
    }

    public class ImageInText : TextElement
    {
        public ImageSource Image;
        public override void SetSize()
        {
            Size.X = Image.Width;
            Size.Y = Image.Height;
        }
        public ImageInText(ImageSource Source)
        {
            ElementType = TextElementType.Image;
            Image = Source;
        }
    }   

    public class BreakElement : TextElement
    {
        public BreakElement() { ElementType = TextElementType.Break; }
    }

    public class TextElement
    {
        public TextElementType ElementType;
        public Vector StartPos;
        public Vector Size;
        public Vector EndPos
        {
            get => StartPos + Size;
        }
        public Brush MarkingColor;
        public virtual void SetSize()
        {
        }
    }

    public enum TextElementType
    {
        Letter,RubyLetter, Image, Break
    }
}
