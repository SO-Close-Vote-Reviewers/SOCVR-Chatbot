using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot
{
    public static class Extensions
    {
        public static string GetContentsWithStrippedMentions(this Message message)
        {
            return ChatExchangeDotNet.ExtensionMethods.StripMention(message.Content);
        }

        public static string GetAllStackTraces(this Exception ex)
        {
            var allStackTraces = GetAllStackTracesInner(ex);

            return allStackTraces
                .ToCSV(Environment.NewLine + "    " + Environment.NewLine);
        }

        private static List<string> GetAllStackTracesInner(this Exception ex)
        {
            List<string> stackTraces = new List<string>();

            if (ex.InnerException != null)
            {
                stackTraces = GetAllStackTracesInner(ex.InnerException);
            }

            var formattedStackTrace = ex.StackTrace
                .Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Select(x => "    " + x)
                .ToCSV(Environment.NewLine);

            stackTraces.Add(formattedStackTrace);

            return stackTraces;
        }

        /// <summary>
        /// Constructs a user-friendly string for this TimeSpan instance.
        /// </summary>
        /// <remarks>
        /// http://www.blackbeltcoder.com/Articles/time/creating-a-user-friendly-timespan-string
        /// </remarks>
        public static string ToUserFriendlyString(this TimeSpan span)
        {
            const int DaysInYear = 365;
            const int DaysInMonth = 30;

            // Get each non-zero value from TimeSpan component
            List<string> values = new List<string>();

            // Number of years
            int days = span.Days;
            if (days >= DaysInYear)
            {
                int years = (days / DaysInYear);
                values.Add(CreateValueString(years, "year"));
                days = (days % DaysInYear);
            }
            // Number of months
            if (days >= DaysInMonth)
            {
                int months = (days / DaysInMonth);
                values.Add(CreateValueString(months, "month"));
                days = (days % DaysInMonth);
            }
            // Number of days
            if (days >= 1)
                values.Add(CreateValueString(days, "day"));
            // Number of hours
            if (span.Hours >= 1)
                values.Add(CreateValueString(span.Hours, "hour"));
            // Number of minutes
            if (span.Minutes >= 1)
                values.Add(CreateValueString(span.Minutes, "minute"));
            // Number of seconds (include when 0 if no other components included)
            if (span.Seconds >= 1 || values.Count == 0)
                values.Add(CreateValueString(span.Seconds, "second"));

            // Combine values into string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < values.Count; i++)
            {
                if (builder.Length > 0)
                    builder.Append((i == (values.Count - 1)) ? " and " : ", ");
                builder.Append(values[i]);
            }
            // Return result
            return builder.ToString();
        }

        /// <summary>
        /// Constructs a string description of a time-span value.
        /// </summary>
        /// <param name="value">The value of this item</param>
        /// <param name="description">The name of this item (singular form)</param>
        private static string CreateValueString(int value, string description)
        {
            return String.Format("{0:#,##0} {1}",
                value, (value == 1) ? description : String.Format("{0}s", description));
        }

        //https://github.com/superlogical/TableParser
        public static string ToStringTable<T>(this IEnumerable<T> values, string[] columnHeaders, params Func<T, object>[] valueSelectors)
        {
            return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
        }

        public static string ToStringTable<T>(this T[] values, string[] columnHeaders, params Func<T, object>[] valueSelectors)
        {
            var arrValues = new string[values.Length + 1, valueSelectors.Length];

            // Fill headers
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                arrValues[0, colIndex] = columnHeaders[colIndex];
            }

            // Fill table rows
            for (int rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    object value = valueSelectors[colIndex].Invoke(values[rowIndex - 1]);

                    arrValues[rowIndex, colIndex] = value != null ? value.ToString() : "null";
                }
            }

            return ToStringTable(arrValues);
        }

        public static string ToStringTable(this string[,] arrValues)
        {
            int[] maxColumnsWidth = GetMaxColumnsWidth(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();

            for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                sb.Append("    "); //my edit to this whole code to make it a code block in chat.
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    // Print cell
                    string cell = arrValues[rowIndex, colIndex];
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0)
                {
                    sb.AppendFormat("     |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }

        public static string ToStringTable<T>(this IEnumerable<T> values, params Expression<Func<T, object>>[] valueSelectors)
        {
            var headers = valueSelectors.Select(func => GetProperty(func).Name).ToArray();
            var selectors = valueSelectors.Select(exp => exp.Compile()).ToArray();
            return ToStringTable(values, headers, selectors);
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expresstion)
        {
            if (expresstion.Body is UnaryExpression)
            {
                if ((expresstion.Body as UnaryExpression).Operand is MemberExpression)
                {
                    return ((expresstion.Body as UnaryExpression).Operand as MemberExpression).Member as PropertyInfo;
                }
            }

            if ((expresstion.Body is MemberExpression))
            {
                return (expresstion.Body as MemberExpression).Member as PropertyInfo;
            }
            return null;
        }
    }
}
