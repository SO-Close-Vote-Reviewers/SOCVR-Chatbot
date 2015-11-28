using System.Text.RegularExpressions;
using SOCVR.Chatbot.Bot.Database;

namespace SOCVR.Chatbot.Bot.ChatbotActions.Triggers
{
    public class CompletedAudit : Trigger
    {
        private Regex ptn = new Regex(@"^passed\s+(?:an?\s+)?(\S+)\s+audit$", RegexObjOptions);

        public override string ActionDescription => null;

        public override string ActionName => "Completed Audit";

        public override string ActionUsage => null;

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var chatUser = chatRoom.GetUser(incommingChatMessage.Author.ID);
            var tagName = RegexMatchingObject
                    .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                    .Groups[1]
                    .Value;

            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            da.InsertCompletedAuditEntry(incommingChatMessage.Author.ID, tagName);
        }
    }
}
