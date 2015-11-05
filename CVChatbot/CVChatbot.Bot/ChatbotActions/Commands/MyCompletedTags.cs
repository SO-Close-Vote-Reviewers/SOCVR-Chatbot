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
using System.Linq;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class MyCompletedTags : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return @"^my completed tags$";
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var completedTags = da.GetUserCompletedTags(incommingChatMessage.Author.ID);

            if (!completedTags.Any())
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any completed tags by you on record. When you run out of items in your filter paste the message into chat here and I'll record it.");
                return;
            }

            var headerMessage = "Showing all tags cleared by you that I have on record:";
            var dataMessage = completedTags
                .ToStringTable(new string[] { "Tag Name", "Times Cleared", "Last Cleared" },
                    x => x.TagName,
                    x => x.TimesCleared,
                    x => x.LastTimeCleared.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));

            chatRoom.PostReplyOrThrow(incommingChatMessage, headerMessage);
            chatRoom.PostMessageOrThrow(dataMessage);
        }

        public override string GetActionName()
        {
            return "My Completed Tags";
        }

        public override string GetActionDescription()
        {
            return "Returns a list of tags you have completed.";
        }

        public override string GetActionUsage()
        {
            return "my completed tags";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
