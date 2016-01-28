using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Configuration;
using SOCVR.Chatbot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class PingReviewers : UserCommand
    {
        public override string ActionDescription =>
            "The bot will send a message with an @reply to all users that have done reviews recently.";

        public override string ActionName => "Ping Reviewers";

        public override string ActionUsage => "ping reviewers <message>";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Owner;

        protected override string RegexMatchingPattern => "^ping reviewers(.+)$";



        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var days = 14;
                int.TryParse(ConfigurationAccessor.PingReviewersDaysBackThreshold, out days);

                var users = db.Users.Where(x => (DateTimeOffset.UtcNow - x.ReviewedItems.Max(t => t.ReviewedOn)).TotalDays <= days);

                if (!users.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"No one has a completed review session in the last {days} days");
                    return;
                }

                var msg = new MessageBuilder();

                var messageFromincomingChatMessage = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Value;

                msg.AppendText(messageFromincomingChatMessage + " ");

                foreach (var u in users)
                {
                    msg.AppendPing(chatRoom.GetUser(u.ProfileId));
                }

                chatRoom.PostMessageOrThrow(msg);
            }
        }
    }
}
