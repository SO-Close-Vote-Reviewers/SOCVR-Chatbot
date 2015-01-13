using CVChatbot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Triggers
{
    public class OutOfCloseVotes : EndingSessionTrigger
    {
        string matchPatternText = @"^(?:> )?You have no more close votes today; come back in (\d+) hours\.$";

        public override bool DoesInputActivateTrigger(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);
            return matchPattern.IsMatch(userMessage.Content);
        }

        public override void RunTrigger(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var success = EndSession(userMessage, chatRoom, null);

            if (success)
            {
                string message = "The review session has been marked as completed. To set the number of items you reviewed use the command `last session edit count <new count>`";
                chatRoom.PostReply(userMessage, message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
