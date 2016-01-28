using System;
using System.Text.RegularExpressions;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    /// <summary>
    /// Shows stats about the last session
    /// </summary>
    public class LastSessionStats : UserCommand
    {
        private Regex ptn = new Regex("^last session stats$", RegexObjOptions);

        public override string ActionDescription => "Shows stats about your last review session.";

        public override string ActionName => "Lass Session Stats";

        public override string ActionUsage => "last session stats";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
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
    }
}
