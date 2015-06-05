using System;
using System.IO;
using System.Linq;
using TCL.Extensions;

namespace CVChatbot.Console
{
    /// <summary>
    /// Used for getting values in the settings.txt file.
    /// </summary>
    public static class SettingsFileAccessor
    {
        /// <summary>
        /// Gets the value from the settings file with the indicated key.
        /// An exception is thrown if the settings file can't be found or the
        /// value cannot be found.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetSettingValue<TValue>(string key)
        {
            if (!File.Exists("settings.txt"))
                throw new InvalidOperationException("Settings file does not exist.");

            var settings = File.ReadAllLines("settings.txt")
                    .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
                    .Select(x => SplitSettingsLine(x))
                    .ToDictionary(x => x[0], x => x[1]);

            return settings[key].Parse<TValue>();
        }

        private static string[] SplitSettingsLine(string line)
        {
            //first, find the location of the first '='
            var firstEqualsIndex = line.IndexOf('=');

            var outputArray = new string[2];
            outputArray[0] = line.Substring(0, firstEqualsIndex);
            outputArray[1] = line.Substring(firstEqualsIndex + 1);

            return outputArray;
        }
    }
}
