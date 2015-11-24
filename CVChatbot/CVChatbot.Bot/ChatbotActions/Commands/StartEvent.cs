using System.Linq;
using System.Text.RegularExpressions;
using TCL.Extensions;

namespace SOCVR.Chatbot.Core.ChatbotActions.Commands
{
    public class StartEvent : UserCommand
    {
        private Regex ptn = new Regex("^(pl(ease|[sz]) )?((start(ing)?( the)? event)|(event start(ed)?))( pl(eas|[sz]))?$", RegexObjOptions);

        public override string ActionDescription =>
            "Shows the current stats from the /review/close/stats page and the next 3 tags to work on.";

        public override string ActionName => "Start Event";

        public override string ActionUsage => "start event";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Owner;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // Get the stats
            var sa = new CloseQueueStatsAccessor();
            var statsMessage = sa.GetOverallQueueStats();

            // Get the next 3 tags
            var tags = SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            if (tags == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            var topTags = tags
                .Take(3)
                .Select(x => $"[tag:{x.Key}]");

            var combinedTags = topTags.ToCSV(", ");

            var tagsMessage = $"The tags to work on are: {combinedTags}.";

            chatRoom.PostMessageOrThrow(statsMessage);
            chatRoom.PostMessageOrThrow(tagsMessage);
        }
    }
}
