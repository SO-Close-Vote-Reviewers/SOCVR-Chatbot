using CVChatbot.Bot.Database;
using System.Linq;
using TCL.Extensions;
using System.Text.RegularExpressions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class PingReviewers : UserCommand
    {
        private Regex ptn = new Regex("^ping reviewers (.+)$");

        public override string ActionDescription =>
            "The bot will send a message with an @reply to all users that have done reviews recently.";

        public override string ActionName => "Ping Reviewers";

        public override string ActionUsage => "ping reviewers <message>";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Owner;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var recipientChatProfileIds = da.GetPingReviewersRecipientList(incommingChatMessage.Author.ID, roomSettings.PingReviewersDaysBackThreshold);

            if (!recipientChatProfileIds.Any())
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "No one has a completed review session in the last {0} days"
                    .FormatInline(roomSettings.PingReviewersDaysBackThreshold));
                return;
            }

            var userNames = recipientChatProfileIds
                .Select(x => chatRoom.GetUser(x).Name)
                .Select(x => "@" + x.Replace(" ", ""));

            var combinedUserNames = userNames.ToCSV(" ");

            var messageFromIncommingChatMessage = RegexMatchingObject
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value;

            var outboundMessage = $"{messageFromIncommingChatMessage} {combinedUserNames}";
            chatRoom.PostMessageOrThrow(outboundMessage);
        }
    }
}
