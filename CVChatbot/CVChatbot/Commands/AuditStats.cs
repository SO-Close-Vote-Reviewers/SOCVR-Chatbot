using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using CVChatbot.Model;
using System.Text.RegularExpressions;
using System.Threading;

namespace CVChatbot.Commands
{
    public class AuditStats : UserCommand
    {
        string matchPatternText = @"^audit stats$";

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
                var totalAuditsCount = db.CompletedAuditEntries
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .Count();

                if (totalAuditsCount == 0)
                {
                    chatRoom.PostReply(userMessage, "I don't have any of your audits on record, so I can't produce any stats for you.");
                    return;
                }

                var groupedTags = db.CompletedAuditEntries
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .GroupBy(x => x.TagName)
                    .Select(x => new
                    {
                        TagName = x.Key,
                        Count = x.Count(),
                        Percent = (x.Count() * 1.0) / totalAuditsCount * 100
                    })
                    .OrderByDescending(x => x.Percent)
                    .ThenByDescending(x => x.Count)
                    .ThenBy(x => x.TagName)
                    .ToList()
                    .Select(x => "    {0}: {1} {2} audits".FormatInline(
                        x.TagName.PadRight(8),
                        (Math.Round(x.Percent, 1).ToString() + "%").PadRight(5),
                        x.Count));

                var message = groupedTags.ToCSV(Environment.NewLine);

                chatRoom.PostReply(userMessage, "Stats of all tracked audits by tag:");
                chatRoom.PostMessage(message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
