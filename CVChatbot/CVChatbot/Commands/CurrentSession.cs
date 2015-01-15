using CVChatbot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class CurrentSession : UserCommand
    {
        string matchPatternText = @"^current session?$";

        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);

            var message = userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();

            return matchPattern.IsMatch(message);
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                //find the latest open session for the user

                var latestOpenSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .Where(x => x.SessionEnd == null)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                if (latestOpenSession == null)
                {
                    chatRoom.PostReply(userMessage, "You don't have an ongoing review session on record.");
                    return;
                }

                var deltaTimeSpan = DateTimeOffset.Now - latestOpenSession.SessionStart;
                var formattedStartTs = latestOpenSession.SessionStart.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

                string message = "Your current review session started {0} ago at {1}"
                    .FormatInline(deltaTimeSpan.ToUserFriendlyString(), formattedStartTs);

                chatRoom.PostReply(userMessage, message);
            }
        }

        public override string GetHelpText()
        {
            return "current session - Tells if the user has an open session or not, and when it started";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
