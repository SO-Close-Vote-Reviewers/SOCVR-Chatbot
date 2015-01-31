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
        /// <summary>
        /// The last CSV revision ID of when fresh tags were fetched.
        /// </summary>
        private static string lastCsvRevID;

        /// <summary>
        /// The last time fresh tag data was fetched.
        /// </summary>
        private static DateTime lastRevIdCheckTime;

        // We have once client session wide.
        private static SedeClient sedeClient = null;

        // We run the query and keep the result for a while.
        private static Dictionary<string, int> tags = null;

        private static string loginEmail;
        private static string loginPassword;

        /// <summary>
        /// A singleton for holing the SEDE client.
        /// </summary>
        private static SedeClient Client
        {
            get
            {
                if (sedeClient == null)
                {
                    sedeClient = new SedeClient(loginEmail, loginPassword);
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

        public override string GetActionDescription()
        {
            return "Get the tag that has the most amount of manageable close queue items from the SEDE query.";
        }

        public override string GetActionName()
        {
            return "Current Tag";
        }

        public override string GetActionUsage()
        {
            return "current tag";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        /// <summary>
        /// Outputs the tag.
        /// </summary>
        /// <param name="incommingChatMessage"></param>
        /// <param name="chatRoom"></param>
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // Sets the static variables for credentials. If the SEDE client is already
            // instantiated then changing these values won't matter.
            loginEmail = roomSettings.Email;
            loginPassword = roomSettings.Password;

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

        protected override string GetRegexMatchingPattern()
        {
            return @"^(what is the )?current tag( pl(ease|z))?\??$";
        }
    }
}
