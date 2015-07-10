/*
 * CVChatbot. Chatbot for the SO Close Vote Reviewers Chat Room.
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





using CVChatbot.Bot.Database;
using System;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class CurrentSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var currentSessionStartTs = da.GetCurrentSessionStartTs(incommingChatMessage.Author.ID);

            if (currentSessionStartTs == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "You don't have an ongoing review session on record.");
            }
            else
            {
                var deltaTimeSpan = DateTimeOffset.Now - currentSessionStartTs.Value;
                var formattedStartTs = currentSessionStartTs.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

                var message = "Your current review session started {0} ago at {1}"
                    .FormatInline(deltaTimeSpan.ToUserFriendlyString(), formattedStartTs);

                chatRoom.PostReplyOrThrow(incommingChatMessage, message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(do i have an? |what is my )?(current|active|review)( review)? session( going( on)?)?\??$";
        }

        public override string GetActionName()
        {
            return "Current Session";
        }

        public override string GetActionDescription()
        {
            return "Tells if the user has an open session or not, and when it started.";
        }

        public override string GetActionUsage()
        {
            return "current session";
        }
    }
}
