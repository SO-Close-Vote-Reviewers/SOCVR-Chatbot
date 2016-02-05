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
            var message = "This is a chat bot for the SO Close Vote Reviewers chat room, developed by the [SOCVR developers](https://github.com/SO-Close-Vote-Reviewers)." +
                "For more information see the [github page](https://github.com/SO-Close-Vote-Reviewers/SOCVR-Chatbot). " +
                "Reply with `{0}` to see a list of commands."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>());
            chatRoom.PostMessageOrThrow(message);
        }
    }
}
