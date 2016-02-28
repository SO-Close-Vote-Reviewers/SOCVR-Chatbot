using System;
using System.Linq;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Configuration;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Admin
{
    internal class PingReviewers : UserCommand
    {
        public override string ActionDescription =>
            "The bot will send a message with an @reply to all users that have done reviews recently.";

        public override string ActionName => "Ping Reviewers";

        public override string ActionUsage => "ping reviewers <message>";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => "^ping reviewers(.+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var days = ConfigurationAccessor.PingReviewersDaysBackThreshold;
                var startDate = DateTimeOffset.UtcNow.Date.AddDays(-days);

                var reviewsInTimeFrame = db.ReviewedItems
                    .Where(x => x.ReviewedOn.Date >= startDate)
                    .ToList();

                if (!reviewsInTimeFrame.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"No one has a completed review session in the last {days} days");
                    return;
                }

                var allProfileIds = reviewsInTimeFrame
                    .Select(x => x.ReviewerId)
                    .Distinct()
                    .Except(new[] { incomingChatMessage.Author.ID }) //exclude yourself
                    .ToList();

                var pingableProfileIds = (from profileId in allProfileIds
                                          join pingableUser in chatRoom.GetPingableUsers() on profileId equals pingableUser.ID
                                          select profileId)
                                         .ToList();

                if (!pingableProfileIds.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "There is no one to ping.");
                    return;
                }

                var msg = new MessageBuilder();

                var messageFromIncomingChatMessage = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Value;

                msg.AppendText(messageFromIncomingChatMessage + " ");

                foreach (var profileId in pingableProfileIds)
                {
                    msg.AppendPing(chatRoom.GetUser(profileId));
                }

                chatRoom.PostMessageOrThrow(msg);
            }
        }
    }
}
