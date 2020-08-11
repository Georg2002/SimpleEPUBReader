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

        public static void Report(string Message, LogType type)
        {
            Message = type.ToString() + ": " + Message;
            Log.Add(Message);
            Console.WriteLine(Message);
            if (Log.Count > 3000)
            {
                Log = Log.Take(50).ToList();
            }
        }

        public static void Report (Exception exception)
        {
            Report(exception.Message, LogType.Error);
        }
    }

    public enum LogType
    {
        Info, Error
    }
}
