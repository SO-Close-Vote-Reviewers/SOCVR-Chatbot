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





using System;
using TCL.Extensions;
using CVChatbot.Bot.Database;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Shows stats about the last session
    /// </summary>
    public class LastSessionStats : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var lastFinishedSession = da.GetLatestCompletedSession(incommingChatMessage.Author.ID);

            if (lastFinishedSession == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "You have no completed review sessions on record, so I can't give you any stats.");
                return;
            }

            var sessionEndedTimeAgo = (DateTimeOffset.Now - lastFinishedSession.SessionEnd.Value);
            var sessionLength = lastFinishedSession.SessionEnd.Value - lastFinishedSession.SessionStart;
            var statMessage = "Your last completed review session ended {0} ago and lasted {1}. ";

            if (lastFinishedSession.ItemsReviewed == null)
            {
                statMessage += "However, the number of reviewed items has not been set. Use the command `{0}` to set the new value."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>());
                statMessage = statMessage.FormatSafely(
                    sessionEndedTimeAgo.ToUserFriendlyString(),
                    sessionLength.ToUserFriendlyString());
            }
            else
            {
                TimeSpan averageTimePerReview;
                var itemsReviewed = lastFinishedSession.ItemsReviewed.Value;
                if (itemsReviewed != 0)
                {
                    averageTimePerReview = new TimeSpan(sessionLength.Ticks / (itemsReviewed));
                }
                else
                {
                    averageTimePerReview = new TimeSpan(0);
                }

                statMessage += "You reviewed {2} items, averaging a review every {3}.";
                statMessage = statMessage.FormatSafely(
                    sessionEndedTimeAgo.ToUserFriendlyString(),
                    sessionLength.ToUserFriendlyString(),
                    lastFinishedSession.ItemsReviewed.Value,
                    averageTimePerReview.ToUserFriendlyString());
            }

            // Check if there is a on-going review session.
            var ongoingSessionStartTs = da.GetCurrentSessionStartTs(incommingChatMessage.Author.ID);

            if (ongoingSessionStartTs != null)
            {
                var deltaTime = DateTimeOffset.Now - ongoingSessionStartTs.Value;
                statMessage += " **Note: You still have a review session in progress.** It started {0} ago.".FormatInline(deltaTime.ToUserFriendlyString());
            }

            chatRoom.PostReplyOrThrow(incommingChatMessage, statMessage);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"\bstat.+\b.+\blast\b.+\bsession\b|\blast\b.+\bsession\b.+\bstat.+\b";
        }

        public override string GetActionName()
        {
            return "Lass Session Stats";
        }

        public override string GetActionDescription()
        {
            return "Shows stats about your last review session.";
        }

        public override string GetActionUsage()
        {
            return "last session stats";
        }
    }
}
