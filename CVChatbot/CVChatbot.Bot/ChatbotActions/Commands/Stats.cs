using System.Text.RegularExpressions;

namespace SOCVR.Chatbot.Core.ChatbotActions.Commands
{
    public class Stats : UserCommand
    {
        private Regex ptn = new Regex("^(close vote )?stats( (please|pl[sz]))?$", RegexObjOptions);

        public override string ActionDescription =>
            "Shows the stats at the top of the /review/close/stats page.";

        public override string ActionName => "Stats";

        public override string ActionUsage => "stats";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var sa = new CloseQueueStatsAccessor();
            var message = sa.GetOverallQueueStats();

            chatRoom.PostMessageOrThrow(message);
        }
    }
}
