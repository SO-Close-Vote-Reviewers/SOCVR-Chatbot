using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartEvent : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return "^start event$";
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // Get the stats
            var sa = new CloseQueueStatsAccessor();
            var statsMessage = sa.GetOverallQueueStats();

            // Get the next 3 tags
            // TODO: this
            var tagsMessage = "The tags to work on are [tag:X], [tag:Y], and [tag:Z].";

            chatRoom.PostMessageOrThrow(statsMessage);
            chatRoom.PostMessageOrThrow(tagsMessage);
        }

        public override string GetActionName()
        {
            return "Start Event";
        }

        public override string GetActionDescription()
        {
            return "Shows the current stats from the /review/close/stats page and the next 3 tags to work on.";
        }

        public override string GetActionUsage()
        {
            return "start event";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
