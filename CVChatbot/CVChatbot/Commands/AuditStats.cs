﻿using System;
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
                    .ToList();

                var message = groupedTags
                    .ToStringTable(new string[] { "Tag Name", "%", "Count"},
                        (x) => x.TagName,
                        (x) => Math.Round(x.Percent, 1),
                        (x) => x.Count);

                chatRoom.PostReply(userMessage, "Stats of all tracked audits by tag:");
                chatRoom.PostMessage(message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetMatchPattern()
        {
            return @"^(show (me )?|display )?(my )?(audit stats|stats (of|about) my audits)$";
        }

        public override string GetCommandName()
        {
            return "Audit Stats";
        }

        public override string GetCommandDescription()
        {
            return "shows stats about your recorded audits";
        }

        public override string GetCommandUsage()
        {
            return "audit stats";
        }
    }
}