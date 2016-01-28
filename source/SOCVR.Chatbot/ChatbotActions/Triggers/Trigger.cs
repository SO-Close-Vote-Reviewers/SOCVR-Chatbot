namespace SOCVR.Chatbot.ChatbotActions.Triggers
{
    /// <summary>
    /// A Trigger is a message said in chat which is not a direct reply to the chatbot, but the chatbot will take action on the message.
    /// </summary>
    internal abstract class Trigger : ChatbotAction
    {
        /// <summary>
        /// Hard-coded to return false.
        /// If you are running a trigger then it can't be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected override sealed bool ReplyMessagesOnly => false;
    }
}
