using ChatExchangeDotNet;
using TCL.Extensions;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class Help : UserCommand
    {
        public override string ActionDescription => "Prints info about this software.";

        public override string ActionName => "Help";

        public override string ActionUsage => "help";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => "^(i need )?(help|assistance|halp|an adult)( me)?( (please|pl[sz]))?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var message = "This is a chat bot for the SO Close Vote Reviewers chat room, developed by [gunr2171](http://stackoverflow.com/users/1043380/gunr2171) and the other members of the SO Close Vote Reviewers chat room. " +
                "For more information see the [github page](https://github.com/SO-Close-Vote-Reviewers/SOCVR-Chatbot). " +
                "Reply with `{0}` to see a list of commands."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>());
            chatRoom.PostMessageOrThrow(message);
        }
    }
}
