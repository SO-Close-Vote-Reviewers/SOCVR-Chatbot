﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;
using System.Text.RegularExpressions;
using System.Threading;
using CVChatbot.Bot.Model;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class AuditStats : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            using (var db = new CVChatBotEntities())
            {
                var totalAuditsCount = db.CompletedAuditEntries
                    .Count(x => x.RegisteredUser.ChatProfileId == incommingChatMessage.AuthorID);

                if (totalAuditsCount == 0)
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any of your audits on record, so I can't produce any stats for you.");
                    return;
                }

                var groupedTags = db.CompletedAuditEntries
                    .Where(x => x.RegisteredUser.ChatProfileId == incommingChatMessage.AuthorID)
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
                    .ToList();

                var message = groupedTags
                    .ToStringTable(new[] { "Tag Name", "%", "Count"},
                        (x) => x.TagName,
                        (x) => Math.Round(x.Percent, 1),
                        (x) => x.Count);

                chatRoom.PostReplyOrThrow(incommingChatMessage, "Stats of all tracked audits by tag:");
                chatRoom.PostMessageOrThrow(message);
            }
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
