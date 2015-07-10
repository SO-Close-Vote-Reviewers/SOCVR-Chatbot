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
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Used for editing the last completed session's review count.
    /// </summary>
    public class LastSessionEditCount : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var lastSession = da.GetLatestCompletedSession(incommingChatMessage.Author.ID);

            if (lastSession == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "You have no completed review sessions on record, so I can't edit any entries.");
                return;
            }

            var newReviewCount = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            if (newReviewCount < 0)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "New review count cannot be negative.");
                return;
            }

            var previousReviewCount = lastSession.ItemsReviewed;
            lastSession.ItemsReviewed = newReviewCount;

            var replyMessage = @"    Review item count has been changed:
    User: {0} ({1})
    Start Time: {2}
    End Time: {3}
    Items Reviewed: {4} -> {5}
    Use the command 'last session stats' to see more details."
                .FormatInline(
                    incommingChatMessage.Author.Name,
                    incommingChatMessage.Author.ID,
                    lastSession.SessionStart.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                    lastSession.SessionEnd.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                    previousReviewCount.HasValue
                        ? previousReviewCount.Value.ToString()
                        : "[Not Set]",
                    lastSession.ItemsReviewed.Value);

            da.EditLatestCompletedSessionItemsReviewedCount(lastSession.Id, newReviewCount);

            chatRoom.PostReplyOrThrow(incommingChatMessage, replyMessage);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^last session edit count (\d+)$";
        }

        public override string GetActionName()
        {
            return "Last Session Edit Count";
        }

        public override string GetActionDescription()
        {
            return "Edits the number of reviewed items in your last review session.";
        }

        public override string GetActionUsage()
        {
            return "last session edit count <new count>";
        }
    }
}
