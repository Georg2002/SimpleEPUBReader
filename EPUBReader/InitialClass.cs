using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using EPUBRenderer;
using System.Runtime.InteropServices;

namespace EPUBReader
{
    public static class InitialClass
    {
#pragma warning disable IDE0060 // Remove unused parameter
        [STAThread]
        public static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            KeepAlive();
            Application app = new App();

            //Create Window
            MainWindow2 win = new MainWindow2(args);

            //Launch the app
            app.Run(win);
        }

        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(EXECUTION_STATE esFlags);
        private static void KeepAlive()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }
    }
}
