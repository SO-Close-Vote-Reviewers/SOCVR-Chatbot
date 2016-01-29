using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

namespace SOCVR.Chatbot.Configuration
{

    internal static class ConfigurationAccessor
    {
        public static string LoginEmail { get { return GetConfigurationOption<string>("LoginEmail"); } }
        public static string LoginPassword { get { return GetConfigurationOption<string>("LoginPassword"); } }
        public static string DatabaseConnectionString { get { return GetConfigurationOption<string>("DatabaseConnectionString"); } }
        public static string ChatRoomUrl { get { return GetConfigurationOption<string>("ChatRoomUrl"); } }

        public static int DefaultCompletedTagsPeopleThreshold { get { return GetConfigurationOption<int>("DefaultCompletedTagsPeopleThreshold"); } }
        public static int MaxFetchTags { get { return GetConfigurationOption<int>("MaxFetchTags"); } }
        public static int PingReviewersDaysBackThreshold { get { return GetConfigurationOption<int>("PingReviewersDaysBackThreshold"); } }
        public static int DefaultNextTagCount { get { return GetConfigurationOption<int>("DefaultNextTagCount"); } }

        public static string StartUpMessage { get { return GetConfigurationOption<string>("StartUpMessage"); } }
        public static string StopMessage { get { return GetConfigurationOption<string>("StopMessage"); } }

        /// <summary>
        /// Searches for a configuration setting by the given key.
        /// First search will be enviornment variables.
        /// Second search will be a "settings.json" file.
        /// If the setting cannot be found in either place an exception is thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private static T GetConfigurationOption<T>(string key)
        {
            //first, check enviornment variables
            var envValue = Environment.GetEnvironmentVariable(key);

            if (!envValue.IsNullOrWhiteSpace())
            {
                return envValue.Parse<T>();
            }

            //now check settings file
            var settingsFilePath = "settings.json";

            if (File.Exists(settingsFilePath))
            {
                var settings = JObject.Parse(File.ReadAllText(settingsFilePath));

                var requestedSettingNode = settings[key];

                if (requestedSettingNode != null)
                {
                    return requestedSettingNode.Value<T>();
                }
            }

            //in neither location, error out
            throw new Exception($"Unable to locate setting '{key}'.");
        }
    }
}
