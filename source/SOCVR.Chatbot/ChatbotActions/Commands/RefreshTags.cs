using System.Text.RegularExpressions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class RefreshTags : UserCommand
    {
        private Regex ptn = new Regex("^refresh tags$", RegexObjOptions);

        public override string ActionDescription =>
            "Forces a refresh of the tags obtained from the SEDE query.";

        public override string ActionName => "Refresh Tags";

        public override string ActionUsage => "refresh tags";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
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
    }
}
