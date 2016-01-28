using System.Text.RegularExpressions;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class StopBot : UserCommand
    {
        public override string ActionDescription =>
            "The bot will leave the chat room and quit the running application.";

        public override string ActionName => "Stop Bot";

        public override string ActionUsage => "stop bot";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Owner;

        protected override string RegexMatchingPattern => "^(stop( bot)?|die|shutdown)$";



        public override void RunAction(Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom) => 
            chatRoom.PostReplyOrThrow(incomingChatMessage, "I'm shutting down...");
    }
}
