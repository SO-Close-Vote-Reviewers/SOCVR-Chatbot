using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class RefreshTags : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return @"refresh tags";
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            SedeAccessor.InvalidateCache();
            SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            chatRoom.PostReplyOrThrow(incommingChatMessage, "Tag data has been refreshed.");
        }

        public override string GetActionName()
        {
            return "Refresh Tags";
        }

        public override string GetActionDescription()
        {
            return "Forces a refresh of the tags obtained from the SEDE query.";
        }

        public override string GetActionUsage()
        {
            return "refresh tags";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
