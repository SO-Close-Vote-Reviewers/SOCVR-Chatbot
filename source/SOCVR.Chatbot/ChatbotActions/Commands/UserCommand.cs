using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    /// <summary>
    /// A User Command is a message sent directly to the chatbot which wants the chatbot to perform an action and reply with a result.
    /// </summary>
    internal abstract class UserCommand : ChatbotAction
    {
        /// <summary>
        /// Hard-coded to return true.
        /// If you want to run a User Command it must be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected override bool ReplyMessagesOnly => true;

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
    }
}
