using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class TotalReviewsToday : UserCommand
    {
        public override string ActionDescription => "Shows summary information and a table of the people who have completed reviews today.";

        public override string ActionName => "Total Reviews Today";

        public override string ActionUsage => "total reviews today";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^total reviews today$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var reviewsToday = db.ReviewedItems
                    .Include(x => x.Reviewer)
                    .Where(x => x.ReviewedOn.Date == DateTimeOffset.UtcNow.Date)
                    .ToList();

                var usersWhoHaveReviewedToday = reviewsToday
                    .GroupBy(x => x.Reviewer)
                    .Select(x => new
                    {
                        ReviewerProfileId = x.Key.ProfileId,
                        ReviewCount = x.Count()
                    })
                    .ToList();

                if (!usersWhoHaveReviewedToday.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I have no record of any reviews from any tracked user today.");
                    return;
                }

                var totalReviewedItems = usersWhoHaveReviewedToday.Sum(x => x.ReviewCount);
                chatRoom.PostReplyOrThrow(incomingChatMessage,
                    $"Today, {usersWhoHaveReviewedToday.Count} member{(usersWhoHaveReviewedToday.Count == 1 ? " has" : "s have")} reviewed a total of {totalReviewedItems} items. They are 55% of the way to processing all review items for the day.");

                var dataTable = usersWhoHaveReviewedToday.ToStringTable(
                    new[]
                    {
                        "User",
                        "Review Items Today"
                    },
                    x => chatRoom.GetUser(x.ReviewerProfileId).Name,
                    x => x.ReviewCount);

                chatRoom.PostMessageOrThrow(dataTable);
            }
        }
    }
}
