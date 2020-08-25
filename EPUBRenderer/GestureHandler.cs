using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EPUBRenderer
{
    public class GestureHandler
    {
        public IInputElement RelativeElement;

        public GestureDefinition Mark = new GestureDefinition()
        {
            StartWait = 500,
            MinVerticalSpeed = -1000,
            MaxVerticalSpeed = 1000,
            MinHorizontalSpeed = -1000,
            MaxHorizontalSpeed = 1000,
            MaxTime = 100000,
            MinTimeBetweenSwitches = 100,
            OneActivationPerTouch = false
        };
        public GestureDefinition SwipeLeft = new GestureDefinition()
        {
            StartWait = 15,
            MinVerticalSpeed = -1000,
            MaxVerticalSpeed = 1000,
            MinHorizontalSpeed = -10000,
            MaxHorizontalSpeed = -2000,
            MaxTime = 100,
            MinTimeBetweenSwitches = 0,
            MinTimeBetweenActivations = 100,
            OneActivationPerTouch = true
        };
        public GestureDefinition SwipeRight = new GestureDefinition()
        {
            StartWait = 15,
            MinVerticalSpeed = -1000,
            MaxVerticalSpeed = 1000,
            MinHorizontalSpeed = 2000,
            MaxHorizontalSpeed = 10000,
            MaxTime = 100,
            MinTimeBetweenSwitches = 0,
            MinTimeBetweenActivations = 100,
            OneActivationPerTouch = true
        };

        private List<GestureDefinition> Gestures;

        private DateTime MouseLeftTouchdown;
        private DateTime LastMove;
        private bool MouseLeftDown;
        private Vector LastMousePos;

        public GestureHandler()
        {
            Gestures = new List<GestureDefinition> { Mark, SwipeLeft, SwipeRight };
        }

        public void CheckActivation()
        {
            var MousePosPoint = Mouse.GetPosition(RelativeElement);
            var MousePos = new Vector(MousePosPoint.X, MousePosPoint.Y);
            if (Mouse.LeftButton == MouseButtonState.Pressed && !MouseLeftDown)
            {
                MouseLeftTouchdown = DateTime.Now;
            }
            MouseLeftDown = Mouse.LeftButton == MouseButtonState.Pressed;
            DateTime Now = DateTime.Now;
            var Delta = MousePos - LastMousePos;
            Delta = Delta / Now.Subtract(LastMove).TotalSeconds;

            foreach (var Gesture in Gestures)
            {
                Gesture.Changed = false;
                bool NewState = MouseLeftDown && Now.Subtract(MouseLeftTouchdown).TotalMilliseconds > Gesture.StartWait
                    && Delta.X > Gesture.MinHorizontalSpeed && Delta.X < Gesture.MaxHorizontalSpeed &&
                    Delta.Y > Gesture.MinVerticalSpeed && Delta.Y < Gesture.MaxVerticalSpeed && !(Gesture.OneActivationPerTouch && Gesture.ActivatedInThisTouch);
                if (NewState && !Gesture.Activated)
                {
                    NewState = Now.Subtract(Gesture.LastSwitch).TotalMilliseconds > Gesture.MinTimeBetweenActivations;
                }

                if (Gesture.Activated != NewState)
                {       
                    if (Now.Subtract(Gesture.LastSwitch).TotalMilliseconds > Gesture.MinTimeBetweenSwitches)
                    {                        
                        Gesture.Activated = NewState;
                        Gesture.LastSwitch = Now;
                        Gesture.Changed = true;
                        Gesture.ActivatedInThisTouch = true;
                    }
                }

                if (!MouseLeftDown)
                {
                    Gesture.ActivatedInThisTouch = false;
                }
            }
            LastMove = Now;
            LastMousePos = MousePos;
        }
    }

    public class GestureDefinition
    {
        //time in ms
        public double StartWait;
        public double MaxTime;
        public double MinTimeBetweenSwitches;
        public double MinTimeBetweenActivations;

        public double MinVerticalSpeed;
        public double MaxVerticalSpeed;
        public double MinHorizontalSpeed;
        public double MaxHorizontalSpeed;

        public bool OneActivationPerTouch;

        public bool ActivatedInThisTouch;

        public bool Changed;
        public bool Activated;
        public DateTime LastSwitch;
    }
}
