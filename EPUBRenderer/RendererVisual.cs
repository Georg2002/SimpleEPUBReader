using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EPUBRenderer
{

    public partial class Renderer : FrameworkElement
    {
        private static readonly Dictionary<Tuple<GlyphTypeface, ushort>, double> WidthDict = new();
        private static readonly object LockObject = new();
        private static double GetAdvanceWidth(ushort index, GlyphTypeface tf)
        {
            if (WidthDict.TryGetValue(new (tf, index), out var width)) return width;
            width = tf.AdvanceWidths[index];
            lock (LockObject)
            {
                WidthDict[new(tf, index)] = width;
            }
            return width;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage == null || !Rerender) return;
            bool SingleImage = ShownPage.IsSingleImage();

            {
                Stopwatch overall = new Stopwatch();
                Stopwatch specialSw = new Stopwatch();

                var offsets = new List<Point>();
                var glyphs = new List<ushort>();
                float size = 0;
                GlyphTypeface tf = null;
                void push()
                {
                    if (glyphs.Any())
                    {
                        var advanceWidths = new double[glyphs.Count];//not used

                        var run = new GlyphRun(
                tf, 0, false, size, 1,
                glyphs, new Point(), advanceWidths,
                offsets, null, null, null, null, null);


                        drawingContext.DrawGlyphRun(Brushes.Black, run);

                    }

                    //everything is passed as reference
                    offsets = new();
                    glyphs = new();
                }



                float prevFontSize = -1;

                overall.Start();
                foreach (TextLetter textLetter in ShownPage.Content.Where(a => a is TextLetter).Cast<TextLetter>())
                {
                    (var letterTf, var glyphIndex) = textLetter.GetRenderingInfo();
                    if (tf != letterTf || prevFontSize != textLetter.FontSize)
                    {
                        push();
                        prevFontSize = textLetter.FontSize;
                        tf = letterTf;
                        size = textLetter.FontSize * textLetter.RelScale;
                    }

                    var drawPos = textLetter.StartPosition + textLetter.Offset * textLetter.FontSize;
                    if (textLetter.Rotated)
                    {
                        push();
                        drawingContext.PushTransform(new RotateTransform(textLetter.Rotation, textLetter.Middle.X, textLetter.Middle.Y));
                    }
                    specialSw.Start();
                                     
                    var width = Renderer.GetAdvanceWidth(glyphIndex, tf);
                    specialSw.Stop();
                    var ul = 0.1;
                    offsets.Add(new Point(drawPos.X - size * (1 + width) / 2, -textLetter.FontSize * (1 - ul) - drawPos.Y));
                    glyphs.Add(glyphIndex);

                    if (textLetter.Rotated)
                    {
                        push();
                        drawingContext.Pop();
                    }

                    if (textLetter.DictSelected && !textLetter.IsRuby)
                    {
                        var Rect = textLetter.GetMarkingRect();
                        drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
                    }

                    if (textLetter.OwnWord.Letters.Last() == textLetter) push();
                }

                overall.Stop();
                Debug.WriteLine(overall.ElapsedMilliseconds);
                Debug.WriteLine(specialSw.ElapsedMilliseconds);
                Debug.WriteLine(specialSw.ElapsedMilliseconds * 100 / (overall.ElapsedMilliseconds + 1) + "%");
            }


            foreach (var Let in ShownPage.Content)
            {
                switch (Let.Type)
                {
                    case LetterTypes.Letter:
                        break;
                    case LetterTypes.Image:
                        var ImgLetter = (ImageLetter)Let;
                        var Img = (ImageSource)ImgLetter.GetImage();
                        var StartPoint = ImgLetter.GetStartPoint();
                        var EndPoint = ImgLetter.GetEndPoint();
                        if (Img == null)
                        {
                            var RedPen = new Pen(Brushes.Red, 1);
                            drawingContext.DrawRectangle(Brushes.Transparent, RedPen, ImgLetter.GetImageRect());
                            drawingContext.DrawLine(RedPen, StartPoint, EndPoint);
                            drawingContext.DrawLine(RedPen, new Point(StartPoint.X, EndPoint.Y), new Point(EndPoint.X, StartPoint.Y));
                        }
                        else
                        {
                            if (SingleImage)
                            {
                                Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;
                            }
                            drawingContext.DrawImage(Img, ImgLetter.GetImageRect());
                        }
                        break;
                    case LetterTypes.Break:
                    case LetterTypes.Marker:
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (Let.MarkingColorIndex != 0)
                {
                    var Rect = Let.GetMarkingRect();
                    drawingContext.DrawRectangle(MarkingColors[Let.MarkingColorIndex], null, Rect);
                }
            }

            int Total = this.GetPageCount();
            int Current = this.GetCurrentPage();
            var PageText = new FormattedText($"{Current}/{Total}", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, CharInfo.StandardTypeface, 15, Brushes.Black, 1);
            double Width = PageText.Width;
            drawingContext.DrawText(PageText, new Point((PageSize.X - Width) / 2, PageSize.Y + 10));
            this.Rerender = false;


        }

        public void ResetSelection()
        {
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
        }
    }
}
