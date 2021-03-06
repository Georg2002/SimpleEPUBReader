﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace EPUBRenderer
{
    public static class Marker
    {
        private static List<MarkingDefinition> TempMarking;
        public static PageRenderer Renderer;

        public static MarkingDefinition GetMarkingAt(RenderPage Page, Point Pos)
        {
            var PosDef = GetElementPosition(Page, Pos);
            if (PosDef == null) return null;
            var Element = Page.TextElements[PosDef.WordIndex][PosDef.ElementIndex];
            if (Element.MarkingColor == null) return null;
            return new MarkingDefinition()
            {
                Position = PosDef,
                Element = Element,
                RenderPageIndex = -1,
                Color = ((SolidColorBrush)Element.MarkingColor).Color
            };
        }

        public static void DeleteMarking(MarkingDefinition Piece, RenderPage Page)
        {
            var Words = Page.TextElements;
            var Pos = new PositionDefinition(Piece.Position.ElementIndex, Piece.Position.WordIndex);
            TextElement Next(int Direction)
            {
                Pos.ElementIndex += Direction;
                if (Pos.ElementIndex < 0)
                {
                    Pos.WordIndex--;
                    if (Pos.WordIndex < 0) return null;
                    Pos.ElementIndex = Words[Pos.WordIndex].Count - 1;
                }
                if (Pos.ElementIndex >= Words[Pos.WordIndex].Count)
                {
                    Pos.WordIndex++;
                    Pos.ElementIndex = 0;
                }
                if (Pos.WordIndex >= Words.Count) return null;
                return Words[Pos.WordIndex][Pos.ElementIndex];
            }
            var Current = Words[Pos.WordIndex][Pos.ElementIndex];

            void SwitchAllMarked(int Direction)
            {
                while (Current != null)
                {
                    if (Current.MarkingColor != null)
                    {
                        Current.MarkingColor = null;
                    }
                    else
                    {
                        break;
                    }
                    Current = Next(Direction);
                }
            }
            SwitchAllMarked(1);
            Pos.ElementIndex = Piece.Position.ElementIndex;
            Pos.WordIndex = Piece.Position.WordIndex;
            Current = Next(-1);
            SwitchAllMarked(-1);
            Renderer.InvalidateVisual();
        }

        public static List<MarkingDefinition> Mark(MarkingCommand Command)
        {
            List<MarkingDefinition> Result = new List<MarkingDefinition>();
            var StartPos = GetElementPosition(Command.Page, Command.Pos1);
            var EndPos = GetElementPosition(Command.Page, Command.Pos2);
            if (StartPos == null || EndPos == null) return Result;
            if (StartPos.WordIndex > EndPos.WordIndex || (StartPos.WordIndex == EndPos.WordIndex && StartPos.ElementIndex > EndPos.ElementIndex))
            {
                var InBetween = StartPos;
                StartPos = EndPos;
                EndPos = InBetween;
            }

            for (int WordIndex = StartPos.WordIndex; WordIndex <= EndPos.WordIndex; WordIndex++)
            {
                var Word = Command.Page.TextElements[WordIndex];
                int Limit = Word.Count - 1;
                int Start = 0;
                if (WordIndex == StartPos.WordIndex)
                {
                    Start = StartPos.ElementIndex;
                }
                if (WordIndex == EndPos.WordIndex)
                {
                    Limit = EndPos.ElementIndex;
                }

                for (int ElementIndex = Start; ElementIndex <= Limit; ElementIndex++)
                {
                    var Element = Word[ElementIndex];
                    Element.MarkingColor = Command.Color;
                    var NewDefinition = new MarkingDefinition()
                    {
                        Element = Element,
                        Position = new PositionDefinition(ElementIndex, WordIndex),
                        RenderPageIndex = Command.RenderPageIndex
                    };
                    Result.Add(NewDefinition);
                }
            }
            Renderer.InvalidateVisual();
            return Result;
        }

        public static bool TempMarkOngoing()
        {
            return TempMarking != null;
        }

        public static void ApplyTempMarking()
        {
            TempMarking = null;
        }

        public static void MarkTemporarly(MarkingCommand Command)
        {
            TempMarking = Mark(Command);
        }

        private static PositionDefinition GetElementPosition(RenderPage Page, Point Point)
        {
            var Offset = Page.CurrentOffset;
            for (int WordIndex = 0; WordIndex < Page.TextElements.Count; WordIndex++)
            {
                var Word = Page.TextElements[WordIndex];
                for (int ElementIndex = 0; ElementIndex < Word.Count; ElementIndex++)
                {
                    var Element = Word[ElementIndex];
                    Vector StartPos = Element.StartPos + Offset;
                    Vector EndPos = Element.EndPos + Offset;
                    if (StartPos.X <= Point.X && Point.X < EndPos.X &&
                        StartPos.Y <= Point.Y && Point.Y < EndPos.Y)
                    {
                        return new PositionDefinition(ElementIndex, WordIndex);
                    }
                }
            }
            return null;
        }

        public static void RemoveTempMarking()
        {
            if (TempMarking == null) return;
            TempMarking.ForEach(a => a.Element.MarkingColor = null);
            TempMarking = null;
            Renderer.InvalidateVisual();
        }

        public static List<MarkingDefinition> GetAllMarkings(List<RenderPage> Pages)
        {
            List<MarkingDefinition> Result = new List<MarkingDefinition>();
            for (int p = 0; p < Pages.Count; p++)
            {
                var Page = Pages[p];
                for (int w = 0; w < Page.TextElements.Count; w++)
                {
                    var Word = Page.TextElements[w];
                    for (int e = 0; e < Word.Count; e++)
                    {
                        var Element = Word[e];
                        if (Element.MarkingColor != null)
                        {
                            var NewMarking = new MarkingDefinition
                            {
                                Color = ((SolidColorBrush)Element.MarkingColor).Color,
                                Position = new PositionDefinition(e, w),
                                RenderPageIndex = p
                            };
                            Result.Add(NewMarking);
                        }
                    }
                }
            }
            return Result;
        }

        public static void ApplyAllMarkings(List<MarkingDefinition> Markings, List<RenderPage> Pages)
        {
            foreach (var Marking in Markings)
            {
                if (Marking.Color.Equals(new Color())) continue;
                if (Marking.RenderPageIndex < Pages.Count)
                {
                    var Page = Pages[Marking.RenderPageIndex];
                    if (Marking.Position.WordIndex < Page.TextElements.Count)
                    {
                        var Word = Page.TextElements[Marking.Position.WordIndex];
                        if (Marking.Position.ElementIndex < Word.Count)
                        {
                            var Element = Word[Marking.Position.ElementIndex];
                            Element.MarkingColor = new SolidColorBrush(Marking.Color);
                        }
                    }
                }
            }
        }
    }

    public class MarkingCommand
    {
        public Point Pos1;
        public Point Pos2;
        public RenderPage Page;
        public Brush Color;
        public int RenderPageIndex;
    }

    public class MarkingDefinition
    {
       [XmlIgnore]
        public int RenderPageIndex;
        [XmlIgnore]
        public PositionDefinition Position;
        [XmlIgnore]
        public TextElement Element;
        [XmlIgnore]
        public Color Color;
        [XmlText]
        public string ShortDef
        {
            get
            {
                return RenderPageIndex.ToString() + ';' +
                    Position.WordIndex.ToString() + ';' +
                    Position.ElementIndex.ToString() + ';' +
                    Color.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                string[] vals = value.Split(';');
                if (vals.Length != 4) return;
                try
                {
                    RenderPageIndex = Convert.ToInt32(vals[0]);
                    Position.WordIndex = Convert.ToInt32(vals[1]);
                    Position.ElementIndex = Convert.ToInt32(vals[2]);
                    Color = (Color)ColorConverter.ConvertFromString(vals[3]);
                }
                catch (Exception) { }
            }
        }

        public MarkingDefinition()
        {
            Position = new PositionDefinition();
        }
    }

    public class PositionDefinition
    {
        public int ElementIndex;
        public int WordIndex;
        public PositionDefinition() { }
        public PositionDefinition(int ElementIndex, int WordIndex)
        {
            this.ElementIndex = ElementIndex;
            this.WordIndex = WordIndex;
        }
    }
}
