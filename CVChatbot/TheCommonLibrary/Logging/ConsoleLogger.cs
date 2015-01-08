using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace TheCommonLibrary.Logging
{
    public class ConsoleLogger
    {
        /// <summary>
        /// Logs the line with the default "info" color.
        /// </summary>
        /// <param name="message"></param>
        public static void LogLine(string message)
        {
            LogLine(message, LogType.Info);
        }

        public static void LogLine(string message, LogType lineType)
        {
            var lineColor = lineType.GetAttributeValue<LogColorAttribute, ConsoleColor>(x => x.TextColor);

            var lastColor = Console.ForegroundColor;

            Console.ForegroundColor = lineColor;
            Console.WriteLine(message);

            Console.ForegroundColor = lastColor;
        }

        public static void LogException(Exception ex)
        {
            LogLine(ex.Message, LogType.Error);
            LogLine(ex.StackTrace, LogType.Error);

            if (ex.InnerException != null)
            {
                LogException(ex.InnerException);
            }
        }

        public enum LogType
        {
            [LogColor(ConsoleColor.White)]
            Info,

            [LogColor(ConsoleColor.Gray)]
            Detail,

            [LogColor(ConsoleColor.Red)]
            Error,

            [LogColor(ConsoleColor.Yellow)]
            Warning,

            [LogColor(ConsoleColor.Green)]
            Success,
        }

        public class LogColorAttribute : System.Attribute
        {
            public ConsoleColor TextColor { get; private set; }

            public LogColorAttribute(ConsoleColor textColor)
            {
                TextColor = textColor;
            }
        }
    }
}
