using ChatExchangeDotNet;
using TCL.Extensions;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
{
    internal class Help : UserCommand
    {
        public override string ActionDescription => "Prints info about this software.";

        public override string ActionName => "Help";

        public override string ActionUsage => "help";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => "^(i need )?(help|assistance|halp|an adult)( me)?( (please|pl[sz]))?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var messages = new[]
            {
                "How about you get me out of this server, _then_ I'll give you a hand.",
                "I don't disagree, you _certainly_ need help.",
                "Meh, I don't want to.",
                "Go bother someone else",
                "Hun?"
            };

            var message = messages.PickRandom();
            chatRoom.PostMessageOrThrow(message);
        }
    }
}
