using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
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
            var lastFinishedSession = da.GetLatestCompletedSession(incommingChatMessage.AuthorID);

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
            var ongoingSessionStartTs = da.GetCurrentSessionStartTs(incommingChatMessage.AuthorID);

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
            return @"^last session stats$";
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
