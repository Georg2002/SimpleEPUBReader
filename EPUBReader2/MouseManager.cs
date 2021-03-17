using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using EPUBRenderer3;

namespace EPUBReader2
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

        public const double BarHeight = 60;
        const double ShrunkDetectionHeight = 0.3 * BarHeight;

        public bool Locked;

        public MouseManager(Border Bar, Grid ContentGrid, Renderer Renderer, MainWindow main)
        {
            MainWindow = main;
            this.Bar = Bar;
            this.ContentGrid = ContentGrid;
            this.Renderer = Renderer;
            const double TimeDown = 150;
            const double TimeUp = 300;
            BarAnimationDown = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(TimeDown)));
            BarAnimationUp = new ThicknessAnimation(new Thickness(0, -BarHeight, 0, 0), new Duration(TimeSpan.FromMilliseconds(TimeUp)));
            ShrinkingAnimation = new ThicknessAnimation(new Thickness(0, BarHeight, 0, 0), new Duration(TimeSpan.FromMilliseconds(TimeDown)));
            ExpandingAnimation = new ThicknessAnimation(new Thickness(0, BarHeight / 2, 0, BarHeight / 2), new Duration(TimeSpan.FromMilliseconds(TimeUp)));
        }

        internal void MouseMove(Point Pos, bool Down, bool RightDown)
        {
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
                AverageSpeed = AverageSpeed * 0.9 + 0.1 * new Vector(MousePos.X - LastMousePos.X, MousePos.Y - LastMousePos.Y) / Delta;
            }
            if (Math.Abs(AverageSpeed.LengthSquared) > 1000000) AverageSpeed = new Vector();
            HandleAnimation();
            HandleMarkings();
            HandleGestures();

            LastMousePos = Pos;
        }

        private void HandleGestures()
        {
            if (MouseDown && !Switched && Math.Abs(AverageSpeed.X) > 400 && Math.Abs(AverageSpeed.Y) < 200 && SSinceTouchdown < 0.1 && SSinceTouchdown > 0.01)
            {
                int Direction = AverageSpeed.X > 0 ? 1 : -1;
                MainWindow.JumpPages(Direction);
                Switched = true;
                AverageSpeed = new Vector();
                MarkingInProgress = false;
            }
            if (Liftup) Switched = false;
        }

        private void HandleMarkings()
        {
            const double MinTime = 0.1;
            Vector MoveSinceTouchdown = new Vector(MousePos.X - MouseDownPos.X, MousePos.Y - MouseDownPos.Y);
            bool Draw = SSinceTouchdown > MinTime || MoveSinceTouchdown.Length > 10;
            if (Draw && AverageSpeed.Length > 200 && SSinceTouchdown < 0.2)
            {
                Draw = false;
            }

            var RelPoint = MainWindow.TranslatePoint(MousePos, Renderer);
            if (RightDown)
            {
                Renderer.RemoveMarking(RelPoint);
                MarkingInProgress = false;
            }
            if (MarkingInProgress)
            {
                if (Liftup)
                {
                    if (Draw)
                    {
                        Renderer.FinishMarking(RelPoint, MainWindow.ColorIndex);
                    }                   
                    MarkingInProgress = false;
                }
                else if (Draw)
                {
                    Renderer.DrawTempMarking(RelPoint, MainWindow.ColorIndex);
                }
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
            else if ((MousePos.Y > 40 || MousePos.Y > 30 && !MovingDown) && !MovingUp)
            {
                Bar.BeginAnimation(Border.MarginProperty, BarAnimationUp);
                ContentGrid.BeginAnimation(Grid.MarginProperty, ExpandingAnimation);
                MovingDown = false;
                MovingUp = true;
            }
        }
    }
}
