using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Admin
{
    internal class AddReview : UserCommand
    {
        public override string ActionDescription => "Manually adds a review to a user. Should only be used for testing.";

        public override string ActionName => "Add Review";

        public override string ActionUsage => "add review [review id] [user id]";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^add review (\d+) (\d+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var reviewItemId = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Value
                    .Parse<int>();

                var reviewerId = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[2]
                    .Value
                    .Parse<int>();

                db.EnsureUserExists(reviewerId);

                var parsedReviewItem = UserTracking.GetUserReviewedItem(reviewItemId, reviewerId);

                db.ReviewedItems.Add(parsedReviewItem);
                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, "Review entered.");
            }
        }
    }
}
