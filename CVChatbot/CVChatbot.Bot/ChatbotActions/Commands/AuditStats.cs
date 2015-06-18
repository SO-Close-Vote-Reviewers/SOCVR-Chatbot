/*
 * ChatExchange.Net. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Linq;
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
