using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using TheCommonLibrary.Extensions;
using CVChatbot.Bot.ChatbotActions.Commands;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    /// <summary>
    /// For when a person does 40 review items and has ran out of review actions.
    /// </summary>
    public class OutOfReviewActions : EndingSessionTrigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var itemsReviewed = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            var sucessful = EndSession(incommingChatMessage, chatRoom, itemsReviewed);

            if (sucessful)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Thanks for reviewing! To see more infomation use the command `{0}`."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()));
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:> )?thank you for reviewing (\d+) close votes today; come back in ([\w ]+) to continue reviewing\.$";
        }

        public override string GetActionName()
        {
            return "Out of Review Actions";
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
