using System.Text.RegularExpressions;
using SOCVR.Chatbot.Configuration;
using SOCVR.Chatbot.Sede;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class RefreshTags : UserCommand
    {
        public override string ActionDescription =>
            "Forces a refresh of the tags obtained from the SEDE query.";

        public override string ActionName => "Refresh Tags";

        public override string ActionUsage => "refresh tags";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override string RegexMatchingPattern => "^refresh tags$";



        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            SedeAccessor.InvalidateCache();
            var dataData = SedeAccessor.GetTags(chatRoom, ConfigurationAccessor.LoginEmail, ConfigurationAccessor.LoginPassword);

            if (dataData == null)
            {
                chatRoom.PostReplyOrThrow(incomingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            chatRoom.PostReplyOrThrow(incomingChatMessage, "Tag data has been refreshed.");
        }
    }
}
