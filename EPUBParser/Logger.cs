using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public static class Logger
    {
        internal static List<string> Log = new List<string>();

        internal static void Report(string Message, LogType type)
        {
            EPUBReader.LogType ReportType;
            switch (type)
            {
                case LogType.Info:
                    ReportType = EPUBReader.LogType.Info;
                    break;
                default:
                    ReportType = EPUBReader.LogType.Error;
                    break;               
            }

            EPUBReader.Logger.Report(Message, ReportType);
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
