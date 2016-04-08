using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using System;
using System.Linq;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class MyAuditStats : UserCommand
    {
        public override string ActionDescription => "Shows stats about your recorded audits.";

        public override string ActionName => "My Audit Stats";

        public override string ActionUsage => "my audit stats";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => "^my audit stats$";

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
                    })
                    .OrderBy(x => x.TagName)
                    .OrderByDescending(x => x.Percent);

                var message = data
                    .ToStringTable(new[] { "Tag Name", "%", "Count" },
                        x => x.TagName,
                        x => Math.Round(x.Percent, 2),
                        x => x.Count);

                var msgText = $"You've completed {auditEntries.Count} audit{(auditEntries.Count == 1 ? "" : "s")}:";
                chatRoom.PostReplyOrThrow(incomingChatMessage, msgText);
                chatRoom.PostMessageOrThrow(message);
            }
        }
    }
}
