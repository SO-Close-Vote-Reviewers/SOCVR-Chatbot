namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Stats : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var sa = new CloseQueueStatsAccessor();
            var message = sa.GetOverallQueueStats();

            chatRoom.PostMessageOrThrow(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^(close vote )?stats( (please|pl[sz]))?$";
        }

        public override string GetActionName()
        {
            return "Stats";
        }

        public override string GetActionDescription()
        {
            return "Shows the stats at the top of the /review/close/stats page.";
        }

        public override string GetActionUsage()
        {
            return "stats";
        }
    }
}
