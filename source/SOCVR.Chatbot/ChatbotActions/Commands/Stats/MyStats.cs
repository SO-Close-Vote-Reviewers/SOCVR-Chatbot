using System;
using System.Linq;
using System.Reflection;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class MyStats : UserCommand
    {
        public override string ActionDescription =>
            "Shows stats about your reviews completed today. Or, if requesting a full data dump (\"reviews today details\"), prints a table of the reviews items you've done today.";

        public override string ActionName => "My Stats";

        public override string ActionUsage => "my stats <details|full>";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^my stats(?: (details|full))?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var currentDate = DateTimeOffset.UtcNow;
                var parsedReviews = db.ReviewedItems
                    .Where(x => x.ReviewerId == incomingChatMessage.Author.ID)
                    .Where(x => x.ReviewedOn.Date == currentDate.Date)
                    .ToList();

                var missingReviewCount = db.DayMissingReviews
                    .Where(x => x.Date == currentDate)
                    .Where(x => x.ProfileId == incomingChatMessage.Author.ID)
                    .Select(x => x.MissingReviewsCount)
                    .SingleOrDefault();

                var totalReviewCount = parsedReviews.Count + missingReviewCount;

                var msg = new MessageBuilder();

                msg.AppendText($"You've reviewed {totalReviewCount} post{(totalReviewCount == 1 ? "" : "s")} today");

                var parsedAuditCount = parsedReviews.Where(x => x.AuditPassed != null).Count();
                var totalAuditCount = parsedAuditCount + missingReviewCount;

                if (totalAuditCount > 0)
                {
                    msg.AppendText($" (of which {totalAuditCount} {(totalAuditCount > 1 ? "were audits" : "was an audit")})");
                }

                msg.AppendText(". ");

                // if you've reviewed more than one item, give the stats about the times and speed.
                if (parsedReviews.Count > 1)
                {
                    var durRaw = parsedReviews.Max(r => r.ReviewedOn) - parsedReviews.Min(r => r.ReviewedOn);
                    var durInf = new TimeSpan((durRaw.Ticks / parsedReviews.Count) * (parsedReviews.Count + 1));
                    var avgInf = TimeSpan.FromSeconds(durInf.TotalSeconds / parsedReviews.Count);

                    msg.AppendText("The time between your first and last review today was ");
                    msg.AppendText(durInf.ToUserFriendlyString());
                    msg.AppendText(", averaging to a review every ");
                    msg.AppendText(avgInf.ToUserFriendlyString());
                    msg.AppendText(". ");
                }

                chatRoom.PostReplyOrThrow(incomingChatMessage, msg);

                var includeDetailsTable = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Success;

                if (includeDetailsTable)
                {
                    var detailsTable = parsedReviews.ToStringTable(new[] { "Item Id", "Action", "Audit", "Completed At" },
                        x => x.ReviewId,
                        x => x.ActionTaken,
                        x => GetFriendlyAuditResult(x.AuditPassed),
                        x => x.ReviewedOn.ToString("yyyy-MM-dd HH:mm:ss UTC"));

                    chatRoom.PostMessageOrThrow(detailsTable);
                }
            }
        }

        private string GetFriendlyAuditResult(bool? res)
        {
            if (res == true)
            {
                return "Passed";
            }
            if (res == false)
            {
                return "Failed";
            }
            return "";
        }
    }
}
