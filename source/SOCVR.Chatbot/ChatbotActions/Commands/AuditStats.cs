using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class AuditStats : UserCommand
    {
        private readonly Regex ptn = new Regex("^(show (me )?|display )?(my )?(audit stats|stats (of|about) my audits)( pl(ease|[sz]))?$", RegexObjOptions);

        public override string ActionDescription => "Shows stats about your recorded audits.";

        public override string ActionName => "Audit Stats";

        public override string ActionUsage => "audit stats";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;

        public override void RunAction(Message incommingChatMessage, Room chatRoom)
        {
            var userId = incommingChatMessage.Author.ID;

            using (var db = new DatabaseContext())
            {
                var auditEntries = db.ReviewedItems
                    .Where(x => x.ReviewerId == userId)
                    .Where(x => x.AuditPassed != null)
                    .ToList();

                if (!auditEntries.Any())
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any of your audits on record, so I can't produce any stats for you.");
                    return;
                }

                var data = auditEntries
                    .GroupBy(x => x.PrimaryTag)
                    .Select(x => new
                    {
                        TagName = x.Key,
                        Count = x.Count(),
                        Percent = x.Count() * 1.0 / auditEntries.Count
                    });

                var message = data
                    .ToStringTable(new[] { "Tag Name", "%", "Count" },
                        x => x.TagName,
                        x => Math.Round(x.Percent, 2),
                        x => x.Count);

                chatRoom.PostReplyOrThrow(incommingChatMessage, "Stats of all tracked audits by tag:");
                chatRoom.PostMessageOrThrow(message);
            }
        }
    }
}
