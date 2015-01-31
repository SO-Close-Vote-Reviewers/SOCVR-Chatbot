using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Implements the current command that takes the first tag from the SEDE query and post it to the room.
    /// </summary>
    public class CurrentTag : UserCommand
    {
        #region Private fields.

        // We run the query and keep the result for a while.
        private static Dictionary<string, int> tags = null;

        // We have once client session wide.
        private static SedeClient sedeClient = null;

        /// <summary>
        /// The last CSV revision ID of when fresh tags were fetched.
        /// </summary>
        private static string lastCsvRevID;

        /// <summary>
        /// The last time fresh tag data was fetched.
        /// </summary>
        private static DateTime lastRevIdCheckTime;

        # endregion



        # region Private class(es).

        /// <summary>
        /// Copied from UI.
        /// </summary>
        private static class SettingsAccessor
        {
            public static string GetSettingValue<TValue>(string key)
            {
                if (!File.Exists("settings.txt"))
                    throw new InvalidOperationException("Settings file does not exist.");

                var settings = File.ReadAllLines("settings.txt")
                        .Where(x => !x.StartsWith("#") && !x.IsNullOrWhiteSpace())
                        .Select(x => x.Split('='))
                        .ToDictionary(x => x[0], x => x[1]);

                return settings[key];
            }
        }

        # endregion



        # region Private properties.

        // Singleton.
        private static SedeClient Client
        {
            get
            {
                if (sedeClient == null)
                {
                    sedeClient = new SedeClient(
                        SettingsAccessor.GetSettingValue<string>("LoginEmail"),
                        SettingsAccessor.GetSettingValue<string>("LoginPassword")
                        );
                }
                return sedeClient;
            }
        }

        // Single instance.
        private static Dictionary<string, int> Tags
        {
            get
            {
                if (tags == null || (DateTime.UtcNow - lastRevIdCheckTime).TotalMinutes > 30)
                {
                    var currentID = "";

                    if ((currentID = Client.GetSedeQueryCsvRevisionId("http://data.stackexchange.com/stackoverflow")) != lastCsvRevID)
                    {
                        lastCsvRevID = currentID;
                        tags = Client.GetTags();
                    }

                    lastRevIdCheckTime = DateTime.UtcNow;
                }
                return tags;
            }
        }

        # endregion



        # region Public methods.

        /// <summary>
        /// Outputs the tag.
        /// </summary>
        /// <param name="incommingChatMessage"></param>
        /// <param name="chatRoom"></param>
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            string dataMessage;
            if (Tags != null)
            {
                dataMessage = "[tag:{0}] ({1})".FormatInline(Tags.First().Key, Tags.First().Value);
            }
            else
            {
                dataMessage = "No tags where retrieved :(";
            }

            chatRoom.PostMessageOrThrow(dataMessage); 
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(what is the )?current tag( pl(ease|z))?\??$";
        }

        public override string GetActionName()
        {
            return "Current Tag";
        }

        public override string GetActionDescription()
        {
            return "Get the tag that has the most amount of manageable close queue items from the [SEDE query](http://data.stackexchange.com/stackoverflow/query/236526/tags-that-can-be-cleared-of-votes).";
        }

        public override string GetActionUsage()
        {
            return "current tag";
        }

        # endregion
    }
}
