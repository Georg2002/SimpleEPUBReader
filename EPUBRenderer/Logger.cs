using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBRenderer
{
    public static class Logger
    {
        public static List<string> Log = new List<string>();

        internal static void Report(string Message, LogType type)
        {
            Message = type.ToString() + ": " + Message;
            Log.Add(Message);
            Console.WriteLine(Message);
            if (Log.Count > 3000)
            {
                Log = Log.Take(50).ToList();
            }
        }

        internal static void Report(Exception exception)
        {
            Report(exception.Message, LogType.Error);
        }
    }

    internal enum LogType
    {
        Info, Error
    }
}
