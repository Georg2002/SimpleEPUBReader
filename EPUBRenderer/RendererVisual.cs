using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Typography.OpenFont;

namespace EPUBRenderer
{

    public partial class Renderer : FrameworkElement
    {
        private static readonly Dictionary<ushort, double> WidthDict = new();
        private static readonly object LockObject = new();
        private static double GetAdvanceWidth(ushort index)
        {
            if (!WidthDict.TryGetValue(index, out double width))
            {
                width = WidthDict[index] = PageFile.LookupTf.GetHAdvanceWidthFromGlyphIndex(index) / 1000.0;
            }
            return width;
        }

        private class GlyphRunData
        {
            public List<Point> offsets = new();
            public List<ushort> glyphs = new();
            public GlyphRun run;
            public GlyphRunData()
            {
            }
        }

        private Dictionary<Tuple<float, GlyphTypeface>, GlyphRunData> RunDict = new();

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ShownPage is null) return;


            bool SingleImage = ShownPage.IsSingleImage();
            if (this.Rendering)
            {
                foreach (var data in this.RunDict.Values)
                {
                    //can't clear after draw call because arrays are as ref
                    data.offsets.Clear();
                    data.glyphs.Clear();
                    data.run = null;
                }
                foreach (TextLetter textLetter in ShownPage.Content.Where(a => a is TextLetter).Cast<TextLetter>())
                {
                    (var letterTf, var glyphIndex) = textLetter.GetRenderingInfo();
                    float size = size = textLetter.FontSize * textLetter.RelScale;

                    var drawPos = textLetter.StartPosition + textLetter.Offset * textLetter.FontSize;

                    var width = Renderer.GetAdvanceWidth(glyphIndex);
                    var ul = 0.1;
                    var offset = new Point(drawPos.X - size * (1 + width) / 2, -textLetter.FontSize * (1 - ul) - drawPos.Y);
                    if (textLetter.Rotated)
                    {
                        drawingContext.PushTransform(new RotateTransform(textLetter.Rotation, textLetter.Middle.X, textLetter.Middle.Y));
                        //glyph run can't give each letter its own rotation, so it has to be handled extra
                        //theoretically all equally rotated letters could be drawn in one call, but offsets need to be transformed
                        var advanceWidths = new double[1];//not used

                        var run = new GlyphRun(
                letterTf, 0, false, size, 1,
                new ushort[] { glyphIndex }, new Point(), advanceWidths,
                new Point[] { offset }, null, null, null, null, null);
                        drawingContext.DrawGlyphRun(Brushes.Black, run);

                        drawingContext.Pop();
                    }
                    else
                    {
                        var key = new Tuple<float, GlyphTypeface>(size, letterTf);
                        if (!RunDict.TryGetValue(key, out var data))
                        {
                            data = new();
                            RunDict[key] = data;
                        }
                        data.offsets.Add(offset);
                        data.glyphs.Add(glyphIndex);
                    }


                    if (textLetter.DictSelected && !textLetter.IsRuby)
                    {
                        var Rect = textLetter.GetMarkingRect();
                        drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
                    }
                }
            }

            foreach (var data in this.RunDict)
            {
                var glyphs = data.Value.glyphs;
                var offsets = data.Value.offsets;
                var size = data.Key.Item1;
                var tf = data.Key.Item2;
                if (glyphs.Any())
                {
                    var advanceWidths = new double[glyphs.Count];//not used

                    data.Value.run ??= new GlyphRun(
           tf, 0, false, size, 1,
           glyphs, new Point(), advanceWidths,
           offsets, null, null, null, null, null);

                    drawingContext.DrawGlyphRun(Brushes.Black, data.Value.run);
                }
            }

            Rect combinedRect = new();
            bool combinationRunning = false;
            var lastColor = -1;
            int x = 0;
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
                    if (combinationRunning)
                    {
                        var rect = Let.GetMarkingRect();
                        if (Let.MarkingColorIndex == lastColor && combinedRect.Left == rect.Left && combinedRect.Right == rect.Right)
                        {
                            combinedRect = new Rect(new Point(combinedRect.Left, combinedRect.Top), new Point(combinedRect.Right, rect.Bottom));
                        }
                        else
                        {
                            drawingContext.DrawRectangle(MarkingColors[1+(x++)%4], null, combinedRect);
                            combinedRect = rect;
                        }
                    }
                    else combinedRect = Let.GetMarkingRect();

                    combinationRunning = true;
                    lastColor = Let.MarkingColorIndex;
                }
                else
                {
                    if (!combinationRunning) continue;

                    drawingContext.DrawRectangle(MarkingColors[1 + (x++) % 4], null, combinedRect);
                    combinationRunning = false;
                }
            }
            if (combinationRunning) drawingContext.DrawRectangle(MarkingColors[1 + (x++) % 4], null, combinedRect);



            int Total = this.GetPageCount();
            int Current = this.GetCurrentPage();
            var PageText = new FormattedText($"{Current}/{Total}", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, CharInfo.StandardTypeface, 15, Brushes.Black, 1);
            double Width = PageText.Width;
            drawingContext.DrawText(PageText, new Point((PageSize.X - Width) / 2, PageSize.Y + 10));
            this.Rendering = false;
        }

        public void ResetSelection()
        {
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
        }
    }
}
