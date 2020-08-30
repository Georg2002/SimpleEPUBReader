using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using EPUBRenderer;

namespace EPUBReader
{
    public static class InitialClass
    {
#pragma warning disable IDE0060 // Remove unused parameter
        [STAThread]
        public static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Application app = new App();

            //Create Window
            MainWindow2 win = new MainWindow2(args);

            //Launch the app
            app.Run(win);
        }
    }
}
