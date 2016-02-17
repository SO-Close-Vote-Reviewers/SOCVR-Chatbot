using System;
using System.IO;
using Newtonsoft.Json.Linq;
using TCL.Extensions;

namespace SOCVR.Chatbot.Configuration
{

    internal static class ConfigurationAccessor
    {
        public static string LoginEmail => GetConfigurationOption<string>("LoginEmail");
        public static string LoginPassword => GetConfigurationOption<string>("LoginPassword");
        public static string DatabaseConnectionString => GetConfigurationOption<string>("DatabaseConnectionString");
        public static string ChatRoomUrl => GetConfigurationOption<string>("ChatRoomUrl");

        public static int DefaultCompletedTagsPeopleThreshold => GetConfigurationOption<int>("DefaultCompletedTagsPeopleThreshold");
        public static int MaxFetchTags => GetConfigurationOption<int>("MaxFetchTags");
        public static int PingReviewersDaysBackThreshold => GetConfigurationOption<int>("PingReviewersDaysBackThreshold");
        public static int DefaultNextTagCount => GetConfigurationOption<int>("DefaultNextTagCount");

        public static string StartUpMessage => GetConfigurationOption<string>("StartUpMessage");
        public static string StopMessage => GetConfigurationOption<string>("StopMessage");

        public static int FailedPermissionRequestCooldownHours => GetConfigurationOption<int>("FailedPermissionRequestCooldownHours");

        public static int RepRequirementToJoinReviewers => GetConfigurationOption<int>("RepRequirementToJoinReviewers");

        /// <summary>
        /// Searches for a configuration setting by the given key.
        /// First search will be environment variables.
        /// Second search will be a "settings.json" file.
        /// If the setting cannot be found in either place an exception is thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private static T GetConfigurationOption<T>(string key)
        {
            //first, check environment variables
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
