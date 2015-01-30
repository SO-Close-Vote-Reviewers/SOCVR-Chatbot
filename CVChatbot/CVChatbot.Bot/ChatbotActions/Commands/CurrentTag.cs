using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
   
    /// <summary>
    /// Implements the current command that takes the first tag from the SEDE query and post it to the room
    /// </summary>
    public class CurrentTag:UserCommand
    {
        /// <summary>
        /// copied from UI ... 
        /// </summary>
        private static class SettingsAccessor
        {
            public static string GetSettingValue<TValue>(string key)
            {
                if (!File.Exists("settings.txt"))
                    throw new InvalidOperationException("Settings file does not exist.");

                var settings = File.ReadAllLines("settings.txt")
                        .Where(x => !x.StartsWith("#"))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Split('='))
                        .ToDictionary(x => x[0], x => x[1]);

                return settings[key];
            }
        }

        //we have once client session wide
        static SedeClient sedeClient = null;

        // singleton
        static SedeClient Client
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

        // we run the query and keep the result for a while
        static Dictionary<string, int> tags = null;
        // single instance
        static Dictionary<string, int> Tags
        {
            get
            {
                // TODO: Expire this! (use a Cache?)
                if (tags == null)
                {
                    // notice that a SEDE Client gets created here
                    // and at least two fetches for the data...
                    tags = Client.GetTags();
                }
                return tags;
            }
        }


        protected override string GetRegexMatchingPattern()
        {
            // does this need to relax?
            return @"^current$";
        }

        /// <summary>
        /// outputs the tag
        /// </summary>
        /// <param name="incommingChatMessage"></param>
        /// <param name="chatRoom"></param>
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            string dataMessage;
            if (Tags !=null)
            {
                dataMessage = String.Format("[tag:{0}] ({1})", Tags.First().Key, Tags.First().Value);
            } else
            {
                dataMessage = "No tags where retrieved :(";
            }
            
            chatRoom.PostMessageOrThrow(dataMessage); 
        }

        public override string GetActionName()
        {
            return "current";
        }

        public override string GetActionDescription()
        {
            return "get the top tag of the sede query";
        }

        public override string GetActionUsage()
        {
            return "current";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
