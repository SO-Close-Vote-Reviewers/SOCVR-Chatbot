using ChatExchangeDotNet;

namespace CVChatbot.Bot.ChatbotActions.Triggers
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
            var message = "You don't need to post this message anymore! When you finish reviewing I should notice it and record the end of the record.";
            chatRoom.PostReplyOrThrow(userMessage, message);
            return true;
        }
    }
}
