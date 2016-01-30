using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class StartingSession : UserCommand
    {
        public override string ActionDescription =>
            "Deprecated - Informs the chatbot that you are starting a new review session.";

        public override string ActionName => "Starting";

        public override string ActionUsage => "starting";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^(?:i'?m )?start(ing|ed)(?: now)?$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var message = "You don't need to run this command anymore! When you start reviewing I should notice it and record the start of the record.";
            chatRoom.PostReplyOrThrow(incomingChatMessage, message);
        }
    }
}
