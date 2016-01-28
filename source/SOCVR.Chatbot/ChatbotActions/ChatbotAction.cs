using System.Text.RegularExpressions;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions
{
    /// <summary>
    /// An action the chatbot will take based on a chat message.
    /// </summary>
    public abstract class ChatbotAction
    {
        public static RegexOptions RegexObjOptions => RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

        /// <summary>
        /// Determines if the incoming chat message activates this action.
        /// </summary>
        /// <param name="incomingMessage">The message said in the chat room.</param>
        /// <param name="isMessageAReplyToChatbot">A precomputed value indicating if the message is a directly reply to the chatbot.</param>
        /// <returns></returns>
        public bool DoesChatMessageActiveAction(Message incomingMessage, bool isMessageAReplyToChatbot)
        {
            // First, check if the message is a reply or not and if the Action accepts that.
            if (isMessageAReplyToChatbot != ReplyMessagesOnly)
                return false;

            // Now regex test it.
            var formattedContents = GetMessageContentsReadyForRegexParsing(incomingMessage);

            return RegexMatchingObject.IsMatch(formattedContents);
        }

        /// <summary>
        /// The DoesChatMessageActiveAction method passes in if the incoming message is
        /// a reply to the chatbot. This method tells what that value MUST be for the
        /// action to be activated. For example, User Commands MUST be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ReplyMessagesOnly { get; }

        /// <summary>
        /// Returns the regex matching object for this action. This is used to determine if the
        /// chat message is appropriate for this action.
        /// </summary>
        /// <returns>The regex object needed for pattern matching with the incoming chat message.</returns>
        protected abstract Regex RegexMatchingObject { get; }

        /// <summary>
        /// Formats the incoming message's contents so that it can be regex matched more reliably.
        /// </summary>
        /// <param name="incommingMessage"></param>
        /// <returns></returns>
        protected abstract string GetMessageContentsReadyForRegexParsing(Message incommingMessage);

        /// <summary>
        /// Runs the core logic to this action.
        /// </summary>
        /// <param name="incommingChatMessage">The chat message received.</param>
        /// <param name="chatRoom">The chat room the message was said in.</param>
        public abstract void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings);

        /// <summary>
        /// Returns the human-friendly name of the chatbot action.
        /// </summary>
        /// <returns></returns>
        public abstract string ActionName { get; }

        /// <summary>
        /// Returns a short description of what the action does. Triggers return null.
        /// </summary>
        /// <returns></returns>
        public abstract string ActionDescription { get; }

        /// <summary>
        /// Returns the usage of the action, including optional arguments. Triggers return null.
        /// </summary>
        /// <returns></returns>
        public abstract string ActionUsage { get; }

        /// <summary>
        /// Returns the minimum permission level needed by a user to run this action.
        /// </summary>
        /// <returns></returns>
        public abstract ActionPermissionLevel PermissionLevel { get; }
    }

    /// <summary>
    /// Describes the permission levels a ChatbotAction can have.
    /// </summary>
    public enum ActionPermissionLevel
    {
        /// <summary>
        /// All people who join the chat room are allowed to run this action.
        /// </summary>
        Everyone,

        /// <summary>
        /// Only people in the tracked users list can run this action.
        /// </summary>
        Registered,

        /// <summary>
        /// Only people in the tracked users list who are labeled as "owner" can run this action.
        /// </summary>
        Owner,
    }
}
