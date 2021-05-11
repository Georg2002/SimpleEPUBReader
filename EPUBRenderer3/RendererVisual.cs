﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{

    public partial class Renderer : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage == null) return;

            bool SingleImage = ShownPage.IsSingleImage();

            foreach (var Line in ShownPage.Lines)
            {
                foreach (var Word in Line.Words)
                {
                    foreach (var Letter in Word.Letters)
                    {           
                        switch (Letter.Type)
                        {
                            case LetterTypes.Letter:
                                var Text = (FormattedText)Letter.GetRenderElement();
                                var TxtLetter = (TextLetter)Letter;
                                var DrawPos = Letter.StartPosition + TxtLetter.Offset * TxtLetter.FontSize;
                                DrawPos.Y -= TxtLetter.FontSize * CharInfo.FontOffset;
                                drawingContext.DrawText(Text, new Point(DrawPos.X - TxtLetter.FontSize / 2, DrawPos.Y));                               
                              
                                break;
                            case LetterTypes.Image:
                                var ImgLetter = (ImageLetter)Letter;
                                var Img = (ImageSource)Letter.GetRenderElement();                               
                                if (SingleImage)
                                {
                                    Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                    ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                    ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;                                    
                                }
                                var StartPoint = new Point(Letter.StartPosition.X, Letter.StartPosition.Y);
                                var EndPoint = new Point(Letter.EndPosition.X, Letter.EndPosition.Y);
                                drawingContext.DrawImage(Img, new Rect(StartPoint, EndPoint));
                               
                                break;
                            case LetterTypes.Break:
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        if (Letter.MarkingColorIndex != 0)
                        {
                            drawingContext.DrawRectangle(MarkingColors[Letter.MarkingColorIndex], null, Letter.GetMarkingRect());
                        }
                    }
                }
            }
        }      
    }
}
