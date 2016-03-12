using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using CsQuery;
using TCL.Extensions;

namespace SOCVR.Chatbot
{
    /// <summary>
    /// Extension methods used in this project.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///  On a CQ dom find an <input name="foo" value="bar" > with the name foo and return bar or null for no match. 
        /// </summary>
        /// <param name="input">CQ instance</param>
        /// <param name="elementName">elementname</param>
        /// <returns>value on the input tag or null</returns>
        /// <remarks>
        /// Stolen from CE.Net. :O
        /// </remarks>
        internal static string GetInputValue(this CQ input, string elementName)
        {
            var fkeyE = input["input"].FirstOrDefault(e => e?.Attributes["name"] == elementName);

            return fkeyE?.Attributes["value"];
        }

        /// <summary>
        /// Attempts to post the message to the chat room. If the message could not be posted an exception will be thrown.
        /// </summary>
        /// <param name="chatRoom"></param>
        /// <param name="message"></param>
        public static void PostMessageOrThrow(this Room chatRoom, object message)
        {
            var success = chatRoom.PostMessageLight(message);

            if (!success)
            {
                throw new InvalidOperationException("Unable to post message");
            }
        }

        /// <summary>
        /// Attempts to post the reply message to the chat room. If the message could not be posted an exception will be thrown.
        /// </summary>
        /// <param name="chatRoom"></param>
        /// <param name="replyingToMessage"></param>
        /// <param name="message"></param>
        public static void PostReplyOrThrow(this Room chatRoom, Message replyingToMessage, object message) =>
            chatRoom.PostReplyOrThrow(replyingToMessage.ID, message);

        /// <summary>
        /// Attempts to post the reply message to the chat room. If the message could not be posted an exception will be thrown.
        /// </summary>
        /// <param name="chatRoom"></param>
        /// <param name="replyingToMessageId"></param>
        /// <param name="message"></param>
        public static void PostReplyOrThrow(this Room chatRoom, int replyingToMessageId, object message)
        {
            var success = chatRoom.PostReplyLight(replyingToMessageId, message);

            if (!success)
            {
                throw new InvalidOperationException("Unable to post message");
            }
        }

        /// <summary>
        /// Recursively gets all stack traces from an exception and any inner exceptions. 
        /// Places an empty line between traces.
        /// The most inner exception's stack trace will be at the top and will unwind.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetAllStackTraces(this Exception ex)
        {
            var allStackTraces = GetAllStackTracesInner(ex);

            return allStackTraces
                .ToCSV(Environment.NewLine + "    " + Environment.NewLine);
        }

        /// <summary>
        /// Returns a more "user friendly" variation of a user's name.
        /// </summary>
        public static string GetChatFriendlyUsername(this User user)
        {
            return user.Name.GetChatFriendlyUsername();
        }

        /// <summary>
        /// Returns a more "user friendly" variation of a user's name.
        /// </summary>
        public static string GetChatFriendlyUsername(this string username)
        {
            var ms = Regex.Matches(username, @"\p{Lu}*\p{Ll}*");
            var matches = new List<string>();
            var name = "";

            foreach (Match m in ms)
            {
                if (string.IsNullOrWhiteSpace(m.Value)) continue;

                matches.Add(m.Value);
            }

            if (matches.All(m => m.Length > 2))
            {
                var avg = matches.Average(x => x.Length);
                if (Math.Abs(matches[0].Length - avg) > avg / 3)
                {
                    name = matches.OrderBy(x => x.Length).First();
                }
                else
                {
                    name = matches[0];
                }
            }
            else
            {
                var spaceCount = 0;
                foreach (var m in matches)
                {
                    if (name.Length - spaceCount > 2) break;
                    name += m + " ";
                    spaceCount++;
                }
            }

            if (name.All(char.IsUpper))
            {
                name = name.ToLowerInvariant();
            }

            return (char.ToUpperInvariant(name[0]) + name.Remove(0, 1)).Trim();
        }



        private static List<string> GetAllStackTracesInner(this Exception ex)
        {
            var stackTraces = new List<string>();

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

            var values = new List<string>();

            Action<int, string> componentProcessor = (int componentValue, string componentName) =>
            {
                if (componentValue > 0)
                {
                    var valueToAdd = $"{componentValue} {componentName}{(componentValue == 1 ? "" : "s")}";
                    values.Add(valueToAdd);
                }
            };

            componentProcessor(span.Days, "day");
            componentProcessor(span.Hours, "hour");
            componentProcessor(span.Minutes, "minute");
            componentProcessor(span.Seconds, "second");

            var builder = new StringBuilder();

            for (int i = 0; i < values.Count; i++)
            {
                //add the item
                builder.Append(values[i]);

                //if there is something after this value
                if (i < values.Count - 1)
                {
                    //is this the second to last value?
                    if (i == values.Count - 2)
                    {
                        //if there is only 2 items in the list
                        if (values.Count == 2)
                        {
                            //you just need an "and" between them
                            builder.Append(" and ");
                        }
                        else //there are 3 or more items
                        {
                            //write ", and"
                            builder.Append(", and ");
                        }
                    }
                    else //not the second to last value, like in the middle of the list
                    {
                        builder.Append(", ");
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates an ascii table from the given IEnumerable.
        /// Taken from https://github.com/superlogical/TableParser
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="columnHeaders"></param>
        /// <param name="valueSelectors"></param>
        /// <returns></returns>
        public static string ToStringTable<T>(this IEnumerable<T> values, string[] columnHeaders, params Func<T, object>[] valueSelectors) =>
            ToStringTable(values.ToArray(), columnHeaders, valueSelectors);

        /// <summary>
        /// Creates an ascii table from the given array.
        /// Take from https://github.com/superlogical/TableParser
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="columnHeaders"></param>
        /// <param name="valueSelectors"></param>
        /// <returns></returns>
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

                    arrValues[rowIndex, colIndex] = value?.ToString() ?? "null";
                }
            }

            return ToStringTable(arrValues);
        }

        /// <summary>
        /// Creates an ascii table from the given array.
        /// Taken from https://github.com/superlogical/TableParser
        /// </summary>
        /// <param name="arrValues"></param>
        /// <returns></returns>
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
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }

            return maxColumnsWidth;
        }

        /// <summary>
        /// Creates an ascii table from the given array.
        /// Taken from https://github.com/superlogical/TableParser
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="valueSelectors"></param>
        /// <returns></returns>
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

    /// <summary>
    /// Helper to throw exceptions to enforce incoming contracts.
    /// </summary>
    public static class ThrowWhen
    {
        /// <summary>
        /// Throws an argument exception if value is null or empty.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <param name="paramName">Friendly name of value.</param>
        public static void IsNullOrEmpty(string value, string paramName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{paramName}' must not be null or empty.", paramName);
            }
        }
    }
}
