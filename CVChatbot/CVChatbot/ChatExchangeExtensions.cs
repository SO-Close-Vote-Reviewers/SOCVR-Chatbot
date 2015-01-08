using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    public static class ChatExchangeExtensions
    {
        public static string GetContentsWithStrippedMentions(this Message message)
        {
            return ChatExchangeDotNet.ExtensionMethods.StripMention(message.Content);
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
    }
}
