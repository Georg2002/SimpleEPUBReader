﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Drawing;

namespace EPUBRenderer
{
    public class RenderPage
    {
        public List<List<TextElement>> TextElements;
        public int CurrentPage = 1;
        public Vector SinglePageOffset;
        public int PageCount;
        public Vector CurrentOffset;

        internal bool IsSingleImage(List<TextElement> Text)
        {
            bool SingleImage = false;
            foreach (var Element in Text)
            {
                if (Element.ElementType == TextElementType.Image)
                {
                    if (SingleImage)
                    {
                        SingleImage = false;
                        break;
                    }
                    else
                    {
                        SingleImage = true;
                    }
                }
                else if (Element.ElementType != TextElementType.Break)
                {
                    SingleImage = false;
                    break;
                }
            }
            return SingleImage;
        }
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
                { LineHeight = FontSize, TextAlignment = TextAlignment.Center };
            }
        }
        public override void SetSize()
        {
            Size.X = FontSize;
            Size.Y = FontSize;
        }
        public Letter(char Letter, double FontSize)
        {
            ElementType = TextElementType.Letter;
            this.FontSize = FontSize;
            this.Text = Letter;
        }
        public override string ToString()
        {
            return Text.ToString();
        }
    }

    public class RubyElement : Letter
    {
        //a ruby word always belongs to the normal word in front of it
        public RubyElement(char Letter, double FontSize) : base(Letter, FontSize)
        {
            ElementType = TextElementType.RubyLetter;
        }
    }

    public class ImageInText : TextElement
    {
        private ImageSource _Image;
        public ImageSource Image
        {
            get => _Image;
            set
            {
                _Image = value;
                SetSize();
            }
        }
        public override void SetSize()
        {
            if (Image == null)
            {
                Size.X = GlobalSettings.ErrorRect.Width;
                Size.Y = GlobalSettings.ErrorRect.Height;
            }
            else
            {
                Size.X = Image.Width;
                Size.Y = Image.Height;
            }
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
        Letter, RubyLetter, Image, Break
    }
}
