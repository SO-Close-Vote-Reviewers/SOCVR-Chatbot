using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Extensions
{
    public static class ExceptionsExtensions
    {
        /// <summary>
        /// Returns the exception's error message, along with all inner exception messages.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FullErrorMessage(this Exception ex)
        {
            return ex.FullErrorMessage(Environment.NewLine);
        }

        /// <summary>
        /// Returns the exception's error message, along with all inner exception messages.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FullErrorMessage(this Exception ex, string delimitor)
        {
            var fullMessage = GetInnerException(ex, 0, delimitor);
            return fullMessage;
        }

        private static string GetInnerException(Exception ex, int level, string delimitor)
        {
            string thisLine = "{0}: {1}".FormatInline(level, ex.Message);

            if (ex.InnerException != null)
            {
                var innerExceptionLine = GetInnerException(ex.InnerException, level + 1, delimitor);
                thisLine += delimitor + innerExceptionLine;
            }

            return thisLine;
        }
    }
}
