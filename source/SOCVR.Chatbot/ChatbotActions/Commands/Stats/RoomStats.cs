using SOCVR.Chatbot.Database;
using System;
using System.Linq;
using ChatExchangeDotNet;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class RoomStats : UserCommand
    {
        public override string ActionDescription => "Shows stats about how effective the room is at processing close vote review items.";

        public override string ActionName => "Room Stats";

        public override string ActionUsage => "room stats <details>";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^room stats( (details|full))?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var details = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Success;

            if (details)
            {
                Details(incomingChatMessage, chatRoom);
            }
            else
            {
                Normal(incomingChatMessage, chatRoom);
            }
        }

        private void Normal(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                //get the number of reviews done today by the entire site
                var sa = new CloseQueueStatsAccessor();
                var stats = sa.GetOverallQueueStats();

                var parsedReviewsToday = db.ReviewedItems
                    .Where(x => x.ReviewedOn.Date == DateTimeOffset.UtcNow.Date)
                    .ToList();

                var missingReviewsToday = db.DayMissingReviews
                    .Where(x => x.Date == DateTimeOffset.UtcNow.Date)
                    .ToList();

                var totalReviews = parsedReviewsToday.Count + missingReviewsToday.Count;

                if (totalReviews == 0)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I don't have any reviews on record for any tracked person today.");
                    return;
                }

                //var reviewerCount = tracker.WatchedUsers.Values.Count(x => x.CompletedReviewsCount > 0);

                var reviewerCount =
                    parsedReviewsToday.Select(x => x.ReviewerId).Distinct().Count() +
                    missingReviewsToday.Select(x => x.ProfileId).Distinct().Count();

                var percentage = Math.Round(totalReviews  * 100D / stats.ReviewsToday, 2);

                var usersPercentage = Math.Round(reviewerCount * 100D / chatRoom.PingableUsers.Count(x => x.Reputation >= 3000));

                var message = $"{reviewerCount} members ({usersPercentage}% of this room's able reviewers) have processed {totalReviews} review items today, which accounts for {percentage}% of all CV reviews today.";

                chatRoom.PostReplyOrThrow(incomingChatMessage, message);
            }
        }

        private void Details(Message incomingChatMessage, Room chatRoom)
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

                var phrase_memberhas = usersWhoHaveReviewedToday.Count == 1
                    ? "member has"
                    : "members have";

                var totalReviewedItems = usersWhoHaveReviewedToday.Sum(x => x.ReviewCount);
                var phrase_item = totalReviewedItems == 1
                    ? "item"
                    : "items";

                chatRoom.PostReplyOrThrow(incomingChatMessage,
                    $"Today, {usersWhoHaveReviewedToday.Count} {phrase_memberhas} reviewed a total of {totalReviewedItems} {phrase_item}.");

                var dataTable = usersWhoHaveReviewedToday
                    .Select(x => new
                    {
                        ReviewCount = x.ReviewCount,
                        ReviewerName = chatRoom.GetUser(x.ReviewerProfileId).Name
                    })
                    .OrderByDescending(x => x.ReviewCount)
                    .ThenBy(x => x.ReviewerName)
                    .ToStringTable(
                        new[]
                        {
                            "User",
                            "Review Items Today"
                        },
                        x => x.ReviewerName,
                        x => x.ReviewCount);

                chatRoom.PostMessageOrThrow(dataTable);
            }
        }
    }
}
