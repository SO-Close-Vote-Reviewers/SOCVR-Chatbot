using CVChatbot.Bot.ChatbotActions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    public class OutOfCloseVotes : EndingSessionTrigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var success = EndSession(incommingChatMessage, chatRoom, null);

            if (success)
            {
                var message = "The review session has been marked as completed. To set the number of items you reviewed use the command `{0}`"
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>());
                chatRoom.PostReplyOrThrow(incommingChatMessage, message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:> )?you have no more close votes today; come back in (\d+) hours\.$";
        }

        public override string GetActionName()
        {
            return "Out of Close Votes";
        }

        public override string GetActionDescription()
        {
            return null;
        }

        public override string GetActionUsage()
        {
            return null;
        }
    }
}
