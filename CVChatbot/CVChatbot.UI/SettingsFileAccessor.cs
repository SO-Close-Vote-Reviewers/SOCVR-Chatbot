using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.UI
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
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], x => x[1]);

            return settings[key].Parse<TValue>();
        }
    }
}
