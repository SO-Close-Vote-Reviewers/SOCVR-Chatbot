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





using System.Linq;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartEvent : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return "^(pl(ease|[sz]) )?((start(ing)?( the)? event)|(event start(ed)?))( pl(eas|[sz]))?$";
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // Get the stats
            var sa = new CloseQueueStatsAccessor();
            var statsMessage = sa.GetOverallQueueStats();

            // Get the next 3 tags
            var tags = SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            if (tags == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            var topTags = tags
                .Take(3)
                .Select(x => "[tag:{0}]".FormatInline(x.Key));

            var combinedTags = topTags.ToCSV(", ");

            var tagsMessage = "The tags to work on are: {0}.".FormatInline(combinedTags);

            chatRoom.PostMessageOrThrow(statsMessage);
            chatRoom.PostMessageOrThrow(tagsMessage);
        } 

        public override string GetActionName()
        {
            return "Start Event";
        }

        public override string GetActionDescription()
        {
            return "Shows the current stats from the /review/close/stats page and the next 3 tags to work on.";
        }

        public override string GetActionUsage()
        {
            return "start event";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
