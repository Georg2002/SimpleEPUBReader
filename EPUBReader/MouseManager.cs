using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using EPUBRenderer;

namespace EPUBReader
{
    class MouseManager
    {
        readonly Border Bar;
        readonly Grid ContentGrid;
        bool MovingDown;
        bool MovingUp;
        readonly ThicknessAnimation BarAnimationDown;
        readonly ThicknessAnimation BarAnimationUp;
        readonly ThicknessAnimation ShrinkingAnimation;
        readonly ThicknessAnimation ExpandingAnimation;
        DateTime LastMove;
        DateTime TouchdownTime;
        double Delta;
        Point MousePos;
        Point LastMousePos;
        Point MouseDownPos;
        bool RightDown;
        bool MouseDown;
        bool Touchdown;
        bool Liftup;
        bool MarkingInProgress;
        readonly Renderer Renderer;
        readonly MainWindow MainWindow;
        Vector AverageSpeed = new Vector();
        bool Switched = false;
        double SSinceTouchdown;
        double ResScal;

        public const double BarHeight = 60;
        double ShrunkDetectionHeight = 0.3 * BarHeight;

        public bool Locked;
        int SwipeDetectCount = 0;
        internal bool DictActive;
        public readonly Thickness BarDownMarg = new Thickness(0, 0, 0, 0);
        public readonly Thickness BarUpMarg = new Thickness(0, -BarHeight, 0, 0);
        public readonly Thickness GridShrinkMarg = new Thickness(0, BarHeight, 0, 0);
        public readonly Thickness GridExpMarg = new Thickness(0, BarHeight / 2, 0, BarHeight / 2);

        public MouseManager(Border Bar, Grid ContentGrid, Renderer Renderer, MainWindow main)
        {
            MainWindow = main;
            this.Bar = Bar;

            this.ContentGrid = ContentGrid;
            this.Renderer = Renderer;
            const double TimeDown = 150;
            const double TimeUp = 300;
            BarAnimationDown = new ThicknessAnimation(BarDownMarg, new Duration(TimeSpan.FromMilliseconds(TimeDown)));
            BarAnimationUp = new ThicknessAnimation(BarUpMarg, new Duration(TimeSpan.FromMilliseconds(TimeUp)));
            ShrinkingAnimation = new ThicknessAnimation(GridShrinkMarg, new Duration(TimeSpan.FromMilliseconds(TimeDown)));
            ExpandingAnimation = new ThicknessAnimation(GridExpMarg, new Duration(TimeSpan.FromMilliseconds(TimeUp)));
        }

        internal void MouseMove(Point Pos, bool Down, bool RightDown)
        {
            if (ResScal == 0) ResScal = PresentationSource.FromVisual(MainWindow).CompositionTarget.TransformToDevice.M11;
            this.RightDown = RightDown;
            Delta = DateTime.Now.Subtract(LastMove).TotalSeconds;
            LastMove = DateTime.Now;
            MousePos = Pos;
            Touchdown = false;
            Liftup = false;

            if (MouseDown != Down)
            {
                Touchdown = Down;
                Liftup = !Down;
                MouseDown = Down;
                if (MouseDown)
                {
                    MouseDownPos = Pos;
                    TouchdownTime = DateTime.Now;
                }
            }
            SSinceTouchdown = DateTime.Now.Subtract(TouchdownTime).TotalSeconds;
            if (Delta != 0)
            {
                AverageSpeed = new Vector(MousePos.X - LastMousePos.X, MousePos.Y - LastMousePos.Y) / Delta;
            }
            if (DictActive) HandleSelection();
            else HandleMarkings();

            HandleGestures();
            HandleAnimation();

            LastMousePos = Pos;
        }

        private void HandleSelection()
        {
            const double MinTime = 0.1;
            bool Draw = SSinceTouchdown > MinTime;
            var RelPoint = MainWindow.TranslatePoint(MousePos, Renderer);
            MarkingInProgress = Switched ? false : MarkingInProgress;

            if (RightDown) MarkingInProgress = false;
            if (MarkingInProgress)
            {
                if (Liftup) MarkingInProgress = false;
                if (Draw && Delta > 0)
                {
                    Renderer.ContinueSelection(RelPoint);
                    MainWindow.Lookup(Renderer.GetSelection());
                }
            }
            else
            {
                if (Touchdown)
                {
                    Renderer.RemoveSelection();
                    bool MarkingValid = Renderer.StartSelection(RelPoint);
                    MarkingInProgress = MarkingValid;
                }
            }
        }

        private void HandleMarkings()
        {
            const double MinTime = 0.1;
            bool Draw = SSinceTouchdown > MinTime;
            var RelPoint = MainWindow.TranslatePoint(MousePos, Renderer);
            MarkingInProgress = Switched ? false : MarkingInProgress;

            if (RightDown)
            {
                Renderer.RemoveMarking(RelPoint);
                MarkingInProgress = false;
            }
            if (MarkingInProgress)
            {
                if (Liftup)
                {
                    if (Draw) Renderer.FinishMarking(RelPoint, MainWindow.ColorIndex);
                    MarkingInProgress = false;
                }
                else if (Draw && Delta > 0) Renderer.DrawTempMarking(RelPoint, MainWindow.ColorIndex);
            }
            else
            {
                if (Touchdown)
                {
                    bool MarkingValid = Renderer.StartMarking(RelPoint);
                    MarkingInProgress = MarkingValid;
                }
            }
        }

        private void HandleGestures()
        {
            SwipeDetectCount = MouseDown ? SwipeDetectCount : 0;
            if (MouseDown && MainWindow.MouseOverText() && !Switched && Math.Abs(AverageSpeed.X) * ResScal > 50 && Math.Abs(AverageSpeed.Y) / Math.Abs(AverageSpeed.X) < 0.25
    && SSinceTouchdown < 0.1 && SSinceTouchdown > 0.01)
            {
                SwipeDetectCount++;
                if (SwipeDetectCount > 2)
                {
                    SwipeDetectCount = 0;
                    int Direction = AverageSpeed.X > 0 ? 1 : -1;
                    MainWindow.JumpPages(Direction);
                    Switched = true;
                    AverageSpeed = new Vector();
                    MarkingInProgress = false;
                    Renderer.ResetSelection();
                    MainWindow.Lookup("");
                }
            }
            else
            {
                SwipeDetectCount--;
                if (SwipeDetectCount < 0) SwipeDetectCount = 0;
            }
            if (Liftup) Switched = false;
        }

        private void HandleAnimation()
        {
            if (Locked) return;
            if (MousePos.Y < ShrunkDetectionHeight && !MovingDown)
            {
                MovingDown = true;
                MovingUp = false;
                Bar.BeginAnimation(Border.MarginProperty, BarAnimationDown);
                ContentGrid.BeginAnimation(Grid.MarginProperty, ShrinkingAnimation);
            }
            else if ((MousePos.Y > BarHeight || MousePos.Y > BarHeight * 0.75 && !MovingDown) && !MovingUp)
            {
                Bar.BeginAnimation(Border.MarginProperty, BarAnimationUp);
                ContentGrid.BeginAnimation(Grid.MarginProperty, ExpandingAnimation);
                MovingDown = false;
                MovingUp = true;
            }
        }

        internal void MoveDown()
        {
            Bar.BeginAnimation(Border.MarginProperty, null);
            ContentGrid.BeginAnimation(Grid.MarginProperty, null);
            Bar.Margin = BarDownMarg;
            ContentGrid.Margin = GridShrinkMarg;
        }
    }
}
