using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;
using System.Text.RegularExpressions;
using System.Threading;
using CVChatbot.Bot.Database;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class AuditStats : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var auditEntries = da.GetUserAuditStats(incommingChatMessage.Author.ID);

            if (!auditEntries.Any())
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any of your audits on record, so I can't produce any stats for you.");
                return;
            }

            var message = auditEntries
                .ToStringTable(new[] { "Tag Name", "%", "Count" },
                    (x) => x.TagName,
                    (x) => Math.Round(x.Percent, 2),
                    (x) => x.Count);

            chatRoom.PostReplyOrThrow(incommingChatMessage, "Stats of all tracked audits by tag:");
            chatRoom.PostMessageOrThrow(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(show (me )?|display )?(my )?(audit stats|stats (of|about) my audits)( pl(ease|[sz]))?$";
        }

        public override string GetActionName()
        {
            return "Audit Stats";
        }

        public override string GetActionDescription()
        {
            return "Shows stats about your recorded audits.";
        }

        public override string GetActionUsage()
        {
            return "audit stats";
        }
    }
}
