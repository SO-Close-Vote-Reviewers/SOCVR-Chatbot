using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Admin
{
    internal class StopBot : UserCommand
    {
        public override string ActionDescription =>
            "The bot will leave the chat room and quit the running application.";

        public override string ActionName => "Stop Bot";

        public override string ActionUsage => "stop bot";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        protected override string RegexMatchingPattern => "^(stop( bot)?|die|shutdown)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom) => 
            chatRoom.PostReplyOrThrow(incomingChatMessage, "I'm shutting down...");
    }
}
