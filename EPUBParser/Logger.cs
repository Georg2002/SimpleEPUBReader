using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public static class Logger
    {
        private static List<string> _Log = new List<string>();
        public static List<string> Log { get => _Log; set => _Log = value; }

        public static void Report(string Message, LogType type)
        {
            Message = type.ToString() + ": " + Message;
            Log.Add(Message);            
        }
    

        internal static void Report(Exception exception)
        {
            Report(exception.Message, LogType.Error);
        }
    }

    public enum LogType
    {
        Info, Error
    }
}
