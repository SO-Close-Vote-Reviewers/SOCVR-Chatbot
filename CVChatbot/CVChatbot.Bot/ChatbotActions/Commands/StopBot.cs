namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StopBot : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            chatRoom.PostReplyOrThrow(incommingChatMessage, "I'm shutting down...");
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Owner;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^stop bot$";
        }

        public override string GetActionName()
        {
            return "Stop Bot";
        }

        public override string GetActionDescription()
        {
            return "The bot will leave the chat room and quit the running application.";
        }

        public override string GetActionUsage()
        {
            return "stop bot";
        }
    }
}
