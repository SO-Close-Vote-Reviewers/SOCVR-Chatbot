using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class NextTags : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return @"^next (\d+) tags$";
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // Get the collection of tags
            // TODO: this

            throw new NotImplementedException();
        }

        public override string GetActionName()
        {
            return "Next Tags";
        }

        public override string GetActionDescription()
        {
            return "Displays the first X tags from the SEDE query to focus on.";
        }

        public override string GetActionUsage()
        {
            return "next <#> tags";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
