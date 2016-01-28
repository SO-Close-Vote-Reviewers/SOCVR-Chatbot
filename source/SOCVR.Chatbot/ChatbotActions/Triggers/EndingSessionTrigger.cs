using System;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Bot.ChatbotActions.Commands;
using SOCVR.Chatbot.Bot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Triggers
{
    public abstract class EndingSessionTrigger : Trigger
    {
        /// <summary>
        /// Records the end of a review session for a user. Returns true if the session was successfully marked as finished.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <param name="chatRoom"></param>
        /// <param name="itemsReviewed"></param>
        /// <returns></returns>
        protected bool EndSession(Message userMessage, Room chatRoom, int? itemsReviewed, InstallationSettings settings)
        {
            var da = new DatabaseAccessor(settings.DatabaseConnectionString);

            // Find the latest session by that user.
            var latestSession = da.GetLatestOpenSessionForUser(userMessage.Author.ID);

            // First, check if there is a session.
            if (latestSession == null)
            {
                chatRoom.PostReplyOrThrow(userMessage, "I don't seem to have the start of your review session on record. I might have not been running when you started, or some error happened.");
                return false;
            }

            // Check if session is greater than [MAX_REVIEW_TIME].
            var maxReviewTimeHours = settings.MaxReviewLengthHours;

            var timeThreshold = DateTimeOffset.Now.AddHours(-maxReviewTimeHours);

            if (latestSession.SessionStart < timeThreshold)
            {
                var timeDelta = DateTimeOffset.Now - latestSession.SessionStart;

                var message = "Your last uncompleted review session was {0} ago. Because it has exceeded my threshold ({1} hours), I can't mark that session with this information. "
                    .FormatInline(timeDelta.ToUserFriendlyString(), maxReviewTimeHours) +
                    "Use the command '{0}' to forcefully end that session."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<EndSession>());

                chatRoom.PostReplyOrThrow(userMessage, message);
                return false;
            }

            // It's all good, mark the info as done.
            da.EndReviewSession(latestSession.Id, itemsReviewed);
            return true;
        }
    }
}
