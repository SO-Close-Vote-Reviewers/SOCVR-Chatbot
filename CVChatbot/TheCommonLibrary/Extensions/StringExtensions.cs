using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TheCommonLibrary.Extensions
{
    /// <summary>
    /// Holds all extension methods that extend a string object
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Formats a string via string.Format() and returns the results.  Makes using string.Format easier.  Throws an exception if
        /// the call fails.
        /// </summary>
        /// <param name="format">The string to be formatted.</param>
        /// <param name="args">The arguments with which to format the string.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatInline(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        /// <summary>
        /// Formats a string via string.Format() and returns the results, if possible.  If the string can't be formatted,
        /// the format itself is returned.  In the worst case, an empty string is returned.  Will not return null.  Will not throw an exception.
        /// </summary>
        /// <param name="format">The string to be formatted.</param>
        /// <param name="args">The arguments with which to format the string.</param>
        /// <returns>The formatted string, or the format itself if the string could not be formatted, or an empty string if something else went wrong.</returns>
        public static string FormatSafely(this string format, params object[] args)
        {
            try
            {
                return string.Format(format, args).EmptyIfNull();
            }
            catch
            {
                try
                {
                    return format.EmptyIfNull();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Returns the string if not null, otherwise returns the empty string.
        /// </summary>
        public static string EmptyIfNull(this string s)
        {
            return s == null ? string.Empty : s;
        }

        /// <summary>
        /// Returns true if the string is null, empty, or pure whitespace.  Returns false if the
        /// string contains any non-whitespace.  Safe to call on null strings (this is guaranteed,
        /// since the method is actually a static extension method).
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string Left(this string s, int numChars)
        {
            if (s == null) throw new ArgumentNullException("s");
            if (numChars < 1) return string.Empty;
            if (numChars > s.Length) return s;
            return s.Substring(0, numChars);
        }

        public static T Parse<T>(this string value)
        {
            // Get default value for type so if string
            // is empty then we can return default value.
            T result = default(T);
            if (!string.IsNullOrEmpty(value))
            {
                // we are not going to handle exception here
                // if you need SafeParse then you should create
                // another method specially for that.
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
                result = (T)tc.ConvertFrom(value);
            } return result;
        }
    }
}
