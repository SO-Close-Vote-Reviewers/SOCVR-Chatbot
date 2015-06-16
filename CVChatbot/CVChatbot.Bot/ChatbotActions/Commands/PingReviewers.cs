using CVChatbot.Bot.Database;
using System.Linq;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class PingReviewers : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return @"^ping reviewers (.+)$";
        }

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

            var messageFromIncommingChatMessage = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value;

            var outboundMessage = "{0} {1}".FormatInline(messageFromIncommingChatMessage, combinedUserNames);
            chatRoom.PostMessageOrThrow(outboundMessage);
        }

        public override string GetActionName()
        {
            return "Ping Reviewers";
        }

        public override string GetActionDescription()
        {
            return "The bot will send a message with an @reply to all users that have done reviews recently.";
        }

        public override string GetActionUsage()
        {
            return "ping reviewers <message>";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Owner;
        }
    }
}
