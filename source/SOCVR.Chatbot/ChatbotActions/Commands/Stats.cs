using SOCVR.Chatbot.Database;
using System.Text.RegularExpressions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class Stats : UserCommand
    {
        public override string ActionDescription =>
            "Shows the stats at the top of the /review/close/stats page.";

        public override string ActionName => "Stats";

        public override string ActionUsage => "stats";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^(close vote )?stats( (please|pl[sz]))?$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var sa = new CloseQueueStatsAccessor();
            var message = sa.GetOverallQueueStats();

            chatRoom.PostMessageOrThrow(message);
        }
    }
}
