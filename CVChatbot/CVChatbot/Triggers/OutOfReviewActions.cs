using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CVChatbot.Model;
using System.Configuration;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Triggers
{
    /// <summary>
    /// For when a person does 40 review items and has ran out of review actions
    /// </summary>
    public class OutOfReviewActions : EndingSessionTrigger
    {
        string matchPatternText = @"^(?:> )?Thank you for reviewing (\d+) close votes today; come back in ([\w ]+) to continue reviewing\.$";

        public override bool DoesInputActivateTrigger(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);
            return matchPattern.IsMatch(userMessage.Content);
        }

        public override void RunTrigger(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            Regex matchPattern = new Regex(matchPatternText);
            var itemsReviewed = matchPattern.Match(userMessage.Content).Groups[1].Value.Parse<int>();

            var sucessful = EndSession(userMessage, chatRoom, itemsReviewed);

            if (sucessful)
            {
                chatRoom.PostReply(userMessage, "Thanks for reviewing! To see more infomation use the command `last session stats`.");
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
