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
            var dataData = SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            if (dataData == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

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
