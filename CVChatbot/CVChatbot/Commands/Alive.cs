using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class Alive : UserCommand
    {
        public override void RunCommand(Message userMessage, Room chatRoom)
        {
            List<string> responsePhrases = new List<string>()
            {
                "I'm alive and kicking!",
                "Still here you guys!",
                "I'm not dead yet!",
            };

            var phrase = responsePhrases.PickRandom();

            chatRoom.PostReply(userMessage, phrase);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetMatchPattern()
        {
            return @"(?:(?:are )?you )?alive\??";
        }

        public override string GetCommandName()
        {
            return "Alive";
        }

        public override string GetCommandDescription()
        {
            return "A simple ping command to test if the bot is running";
        }
    }
}
