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
