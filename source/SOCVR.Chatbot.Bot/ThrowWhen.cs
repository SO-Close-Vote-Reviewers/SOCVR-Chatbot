using System;

namespace SOCVR.Chatbot.Bot
{
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
