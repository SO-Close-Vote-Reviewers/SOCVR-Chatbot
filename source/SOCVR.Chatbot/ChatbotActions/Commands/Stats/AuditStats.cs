using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using System;
using System.Linq;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class AuditStats : UserCommand
    {
        public override string ActionDescription => "Shows stats about your recorded audits.";

        public override string ActionName => "Audit Stats";

        public override string ActionUsage => "audit stats";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => "^audit stats$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var userId = incomingChatMessage.Author.ID;

            using (var db = new DatabaseContext())
            {
                var auditEntries = db.ReviewedItems
                    .Where(x => x.ReviewerId == userId)
                    .Where(x => x.AuditPassed != null)
                    .ToList();

                if (!auditEntries.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I don't have any of your audits on record, so I can't produce any stats for you.");
                    return;
                }

                var data = auditEntries
                    .GroupBy(x => x.PrimaryTag)
                    .Select(x => new
                    {
                        TagName = x.Key,
                        Count = x.Count(),
                        Percent = x.Count() * 1.0 / auditEntries.Count * 100
                    });

                var message = data
                    .ToStringTable(new[] { "Tag Name", "%", "Count" },
                        x => x.TagName,
                        x => Math.Round(x.Percent, 2),
                        x => x.Count);

                chatRoom.PostReplyOrThrow(incomingChatMessage, "Stats of all tracked audits by tag:");
                chatRoom.PostMessageOrThrow(message);
            }
        }
    }
}
