using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using System.Text.RegularExpressions;
using System;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    /// <summary>
    /// A User Command is a message sent directly to the chatbot which wants the chatbot to perform an action and reply with a result.
    /// </summary>
    internal abstract class UserCommand : ChatbotAction
    {
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
        /// Returns the permission group the user must be in to run the command.
        /// Null means the user does not need to belong to a permission group to run the command.
        /// </summary>
        /// <returns></returns>
        public abstract PermissionGroup? RequiredPermissionGroup { get; }

        /// <summary>
        /// If true, the user needs to be in at least one permission group to run the command.
        /// If false, the user does not need to be in any permission groups.
        /// This is used when commands are "non-public" - they are not tied to any particular
        /// permission group but they require that you be in at least one of the groups.
        /// </summary>
        public abstract bool UserMustBeInAnyPermissionGroupToRun { get; }

        /// <summary>
        /// This is already populated with the necessary matching pattern text.
        /// You may call this method from the RunAction() method if you need arguments within chat message.
        /// </summary>
        /// <returns>The regex object needed for pattern matching with the incoming chat message.</returns>
        protected Regex GetRegexMatchingObject()
        {
            return new Regex(RegexMatchingPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Returns the regex matching pattern for this action. This is used to determine if the
        /// chat message is appropriate for this action.
        /// </summary>
        /// <returns></returns>
        protected abstract string RegexMatchingPattern { get; }

        /// <summary>
        /// Determines if the incoming chat message activates this action.
        /// </summary>
        /// <param name="incomingMessage">The content of the message said in the chat room.</param>
        /// <param name="isMessageAReplyToChatbot">A precomputed value indicating if the message is a direct reply to the chatbot.</param>
        /// <returns></returns>
        public bool DoesChatMessageActiveAction(string incomingMessage, bool isMessageAReplyToChatbot)
        {
            // First, check if the message is a reply or not and if the Action accepts that.
            if (!isMessageAReplyToChatbot)
                return false;

            // Now regex test it.
            var regex = GetRegexMatchingObject();

            return regex.IsMatch(incomingMessage);
        }

        /// <summary>
        /// Determines if the incoming chat message activates this action.
        /// </summary>
        /// <param name="incomingMessage">The message said in the chat room.</param>
        /// <param name="isMessageAReplyToChatbot">A precomputed value indicating if the message is a direct reply to the chatbot.</param>
        /// <returns></returns>
        public bool DoesChatMessageActiveAction(Message incomingMessage, bool isMessageAReplyToChatbot)
        {
            return DoesChatMessageActiveAction(incomingMessage.Content, isMessageAReplyToChatbot);
        }

        /// <summary>
        /// Runs the core logic to this action.
        /// </summary>
        /// <param name="incomingChatMessage">The chat message received.</param>
        /// <param name="chatRoom">The chat room the message was said in.</param>
        public abstract void RunAction(Message incomingChatMessage, Room chatRoom);

        public sealed override void RunAction(Room chatRoom)
        {
            throw new NotImplementedException();
        }
    }
}
