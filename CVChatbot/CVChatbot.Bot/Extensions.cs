/*
 * CVChatbot. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using ChatExchangeDotNet;
using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using TCL.Extensions;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace CVChatbot.Bot
{
    /// <summary>
    /// Extension methods used in this project.
    /// </summary>
    public static class Extensions
    {
        private static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();



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
            var fkeyE = input["input"].FirstOrDefault(e => e.Attributes["name"] != null && e.Attributes["name"] == elementName);

            return fkeyE == null ? null : fkeyE.Attributes["value"];
        }

        /// <summary>
        /// Takes a chat message and return its contents with any "mentions" stripped out.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns></returns>
        public static string GetContentsWithStrippedMentions(this Message message)
        {
            return Message.GetMessageContent(message.Host, message.ID);
        }

        /// <summary>
        /// Attempts to post the message to the chat room. If the message could not be posted an exception will be thrown.
        /// </summary>
        /// <param name="chatRoom"></param>
        /// <param name="message"></param>
        public static void PostMessageOrThrow(this Room chatRoom, object message)
        {
            var success = chatRoom.PostMessageFast(message);

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
        public static void PostReplyOrThrow(this Room chatRoom, Message replyingToMessage, object message)
        {
            chatRoom.PostReplyOrThrow(replyingToMessage.ID, message);
        }

        /// <summary>
        /// Attempts to post the reply message to the chat room. If the message could not be posted an exception will be thrown.
        /// </summary>
        /// <param name="chatRoom"></param>
        /// <param name="replyingToMessageId"></param>
        /// <param name="message"></param>
        public static void PostReplyOrThrow(this Room chatRoom, int replyingToMessageId, object message)
        {
            var success = chatRoom.PostReplyFast(replyingToMessageId, message);

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
            const int DaysInYear = 365;
            const int DaysInMonth = 30;

            // Get each non-zero value from TimeSpan component
            var values = new List<string>();

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
            var builder = new StringBuilder();
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

        public static string GetChatFriendlyUsername(this User user)
        {
            var ms = Regex.Matches(user.Name, @"\p{Lu}?\p{Ll}*");
            var name = "";

            foreach (Match m in ms)
            {
                if (name.Length > 3) { break; }
                name += m.Value;
            }

            return name;
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
        public static string ToStringTable<T>(this IEnumerable<T> values, string[] columnHeaders, params Func<T, object>[] valueSelectors)
        {
            return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
        }

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

                    arrValues[rowIndex, colIndex] = value != null ? value.ToString() : "null";
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
}
