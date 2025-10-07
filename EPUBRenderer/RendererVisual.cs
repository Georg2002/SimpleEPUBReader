using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;


namespace EPUBRenderer
{

    public partial class Renderer : SKElementCust
    {
        private static readonly Dictionary<ushort, double> WidthDict = new();
        private static readonly object LockObject = new();

        private class GlyphRunData
        {
            public List<SKPoint> offsets = new();
            public List<int> codepoints = new();
            public SKTextBlob run;
            public GlyphRunData()
            {
            }
        }

        private Dictionary<Tuple<float, SKTypeface>, GlyphRunData> RunDict = new();
        private object renderLockObject = new();
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            lock (this.renderLockObject)
            {
                if (ShownPage is null || !this.Rendering) return;
            }
           
            Debug.WriteLine("Render executed");
            var canvas = e.Surface.Canvas;

            // make sure the canvas is blank
            canvas.Clear(SKColors.White);

            bool SingleImage = ShownPage.IsSingleImage();

            foreach (var data in this.RunDict.Values)
            {
                //can't clear after draw call because arrays are as ref
                data.offsets.Clear();
                data.codepoints.Clear();
                data.run = null;
            }

            foreach (TextLetter textLetter in ShownPage.Content.Where(a => a is TextLetter).Cast<TextLetter>())
            {
                (var letterTf, var codepoint) = textLetter.GetRenderingInfo();
                float size = size = textLetter.FontSize * textLetter.RelScale;

                var drawPos = textLetter.StartPosition + textLetter.Offset * textLetter.FontSize;

                var ul = 0.1;
                var offset = new Point(drawPos.X, +textLetter.FontSize * (1 - ul) + drawPos.Y);
                if (textLetter.Rotated)
                {
                    var x = (float)textLetter.Middle.X;
                    var y = (float)textLetter.Middle.Y;
                    canvas.RotateDegrees(textLetter.Rotation, x, y);
                    //glyph run can't give each letter its own rotation, so it has to be handled extra
                    //theoretically all equally rotated letters could be drawn in one call, but offsets need to be transformed

                    using var font = this.GetFont(letterTf, size);
                    canvas.DrawText(textLetter.Character.ToString(), offset.ToSKPoint(), SKTextAlign.Center, font, this.blackPaint);

                    canvas.RotateDegrees(-textLetter.Rotation, x, y);
                }
                else
                {
                    var key = new Tuple<float, SKTypeface>(size, letterTf);
                    if (!RunDict.TryGetValue(key, out var data))
                    {
                        data = new();
                        RunDict[key] = data;
                    }
                    data.offsets.Add(offset.ToSKPoint());
                    data.codepoints.Add(codepoint);
                }


                if (textLetter.DictSelected && !textLetter.IsRuby)
                {
                    var Rect = textLetter.GetMarkingRect();
                    canvas.DrawRect(Rect.ToSKRect(), Letter.DictSelectionPaint);
                }
            }


            foreach (var data in this.RunDict)
            {
                var offsets = data.Value.offsets;
                var size = data.Key.Item1;
                var tf = data.Key.Item2;
                var glyphs = tf.GetGlyphs(data.Value.codepoints.ToArray());
                if (glyphs.Any())
                {
                    using var font = this.GetFont(tf, size);
                    var widths = font.GetGlyphWidths(glyphs);
                    for (int i = 0; i < offsets.Count; i++) offsets[i] = new(offsets[i].X - (size + widths[i]) / 2, offsets[i].Y);

                    if (data.Value.run is null)
                    {
                        var builder = new SKTextBlobBuilder();
                        builder.AddPositionedRun(glyphs, font, offsets.ToArray());
                        data.Value.run = builder.Build();
                    }

                    canvas.DrawText(data.Value.run, 0, 0, blackPaint);
                }
            }

            Rect combinedRect = new();
            bool combinationRunning = false;
            var lastColor = -1;
            foreach (var Let in ShownPage.Content)
            {
                switch (Let.Type)
                {
                    case LetterTypes.Letter:
                        break;
                    case LetterTypes.Image:
                        var ImgLetter = (ImageLetter)Let;
                        var Img = ImgLetter.GetImage();
                        var StartPoint = ImgLetter.GetStartPoint();
                        var EndPoint = ImgLetter.GetEndPoint();
                        if (Img == null)
                        {
                            using var redPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true, Style = SKPaintStyle.Fill };
                            // drawingContext.DrawRectangle(Brushes.Transparent, RedPaint, ImgLetter.GetImageRect());
                            canvas.DrawLine(StartPoint.ToSKPoint(), EndPoint.ToSKPoint(), redPaint);
                            canvas.DrawLine(new Point(StartPoint.X, EndPoint.Y).ToSKPoint(), new Point(EndPoint.X, StartPoint.Y).ToSKPoint(), redPaint);
                        }
                        else
                        {
                            if (SingleImage)
                            {
                                Vector RenderSize = ImgLetter.GetMaxRenderSize(PageSize);
                                ImgLetter.StartPosition = (PageSize - RenderSize) / 2;
                                ImgLetter.EndPosition = ImgLetter.StartPosition + RenderSize;
                            }
                            canvas.DrawImage(Img, ImgLetter.GetImageRect().ToSKRect(), this.samplingOptions);
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
                            canvas.DrawRect(combinedRect.ToSKRect(), MarkingColors[lastColor]);
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

                    canvas.DrawRect(combinedRect.ToSKRect(), MarkingColors[lastColor]);
                    combinationRunning = false;
                }
            }
            if (combinationRunning) canvas.DrawRect(combinedRect.ToSKRect(), MarkingColors[lastColor]);



            int Total = this.GetPageCount();
            int Current = this.GetCurrentPage();
            canvas.DrawText($"{Current}/{Total}", new SKPoint((float)PageSize.X / 2.0f, (float)PageSize.Y + 20.0f), SKTextAlign.Center, CharInfo.StandardFont, this.blackPaint);
            lock (this.renderLockObject) this.Rendering = false;

            this.renderingSemaphore.Release();
        }

        public void ResetSelection()
        {
            this.SelectionStart = PosDef.InvalidPosition;
            this.SelectionEnd = PosDef.InvalidPosition;
        }
    }
}
