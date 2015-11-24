using System.Linq;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.Core.Database;

namespace SOCVR.Chatbot.Core.ChatbotActions.Commands
{
    public class MyCompletedTags : UserCommand
    {
        private Regex ptn = new Regex("^my completed tags$", RegexObjOptions);

        public override string ActionDescription => "Returns a list of tags you have completed.";

        public override string ActionName => "My Completed Tags";

        public override string ActionUsage => "my completed tags";

        protected override Regex RegexMatchingObject => ptn;

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var completedTags = da.GetUserCompletedTags(incommingChatMessage.Author.ID);

            if (!completedTags.Any())
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any completed tags by you on record. When you run out of items in your filter paste the message into chat here and I'll record it.");
                return;
            }

            var headerMessage = "Showing all tags cleared by you that I have on record:";
            var dataMessage = completedTags
                .ToStringTable(new string[] { "Tag Name", "Times Cleared", "Last Cleared" },
                    x => x.TagName,
                    x => x.TimesCleared,
                    x => x.LastTimeCleared.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));

            chatRoom.PostReplyOrThrow(incommingChatMessage, headerMessage);
            chatRoom.PostMessageOrThrow(dataMessage);
        }
    }
}
