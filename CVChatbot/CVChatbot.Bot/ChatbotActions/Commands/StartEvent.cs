using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

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
            SedeAccessor.SetCredentials(roomSettings.Email, roomSettings.Password);
            var tags = SedeAccessor.Tags;

            var topTags = tags
                .Take(3)
                .Select(x => "[tag:{0}]".FormatInline(x.Key));

            var combinedTags = topTags.ToCSV(", ");

            var tagsMessage = "The tags to work on are: {0}.".FormatInline(combinedTags);

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
