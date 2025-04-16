using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Windows.Media.TextFormatting;
using EPUBRenderer;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Security.Policy;
using System.Windows.Input;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using Accessibility;
using System.Windows.Media.Media3D;

namespace EPUBRenderer
{

    public partial class Renderer : HwndHost
    {
        private MouseButtonState leftState;
        private MouseButtonState middleState;
        private MouseButtonState rightState;
        public delegate void MouseMovedHandler(Point pos, bool leftDown, bool rightDown);
        public event MouseMovedHandler MouseMoved;
        private Point throwEvent(IntPtr lParam)
        {
            Point pos;
            unsafe
            {
                var ptr = (int)lParam.ToPointer();
                pos.X = ptr & (int)ushort.MaxValue;
                pos.Y = ptr >> 16;
            }
            pos = this.TranslatePoint(pos, Application.Current.MainWindow);
            MouseMoved?.Invoke(pos, this.leftState == MouseButtonState.Pressed, this.rightState == MouseButtonState.Pressed);
            return pos;
        }
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (hwnd != this.Handle) return this.Handle;
            handled = false;
            switch ((MessageEnum)msg)
            {
                case MessageEnum.MouseMove:

                    //  args.Source = this;
                    Debug.WriteLine($"Moved!: {throwEvent(lParam)}");
                    break;
                case MessageEnum.LeftButtonDown:
                    leftState = MouseButtonState.Pressed;
                    throwEvent(lParam);
                    Debug.WriteLine("Left Down!");
                    break;
                case MessageEnum.LeftButtonUp:
                    leftState = MouseButtonState.Released;
                    throwEvent(lParam);
                    Debug.WriteLine("Left Up!");
                    break;
                case MessageEnum.LeftButtonDoubleClick:
                    Debug.WriteLine("Left Double!");
                    throwEvent(lParam);
                    break;
                case MessageEnum.RightButtonDown:
                    rightState = MouseButtonState.Pressed;
                    Debug.WriteLine("Right Down!");
                    throwEvent(lParam);
                    break;
                case MessageEnum.RightButtonUp:
                    rightState = MouseButtonState.Released;
                    Debug.WriteLine("Right Up!");
                    throwEvent(lParam);
                    break;
                case MessageEnum.RightButtonDoubleClick:
                    Debug.WriteLine("Right Double!");
                    throwEvent(lParam);
                    break;
                case MessageEnum.MiddleButtonDown:
                    middleState = MouseButtonState.Pressed;
                    throwEvent(lParam);
                    break;
                case MessageEnum.MiddleButtonUp:
                    middleState = MouseButtonState.Released;
                    throwEvent(lParam);
                    break;
                case MessageEnum.MiddleButtonDoubleClick:
                    throwEvent(lParam);
                    break;
                case MessageEnum.MouseLeave:
                    leftState = rightState = middleState = MouseButtonState.Released;
                    Debug.WriteLine("Left!");
                    throwEvent(lParam);
                    break;
                default:
                    handled = false;
                    base.WndProc(hwnd, msg, wParam, lParam, ref handled);
                    return this.Handle;
            }
            //HandleMessages((uint)msg, wParam, lParam);

            return this.Handle;
            // return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
