using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader
{
    public static class Logger
    {
        public static List<string> Log = new List<string>();

        public static void Report(string Message)
        {
            Log.Add(Message);
            Console.WriteLine(Message);
        }

        public static void Report (Exception exception)
        {
            Report(exception.Message);
        }
    }
}
