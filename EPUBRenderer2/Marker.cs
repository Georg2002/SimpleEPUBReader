using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
                Color = (SolidColorBrush)Element.MarkingColor
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
            return null;
        }

        public static void ApplyAllMarkings(List<MarkingDefinition> Markings)
        {

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
        public int RenderPageIndex;
        public PositionDefinition Position;
        public TextElement Element;
        public SolidColorBrush Color;
    }

    public class PositionDefinition
    {
        public int ElementIndex;
        public int WordIndex;
        public PositionDefinition(int ElementIndex, int WordIndex)
        {
            this.ElementIndex = ElementIndex;
            this.WordIndex = WordIndex;
        }
    }
}
