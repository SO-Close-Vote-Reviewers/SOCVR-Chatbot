using System;
using System.Linq;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class ReviewsToday : UserCommand
    {
        public override string ActionDescription =>
            "Shows stats about your reviews completed today. Or, if requesting a full data dump (`review today details`), prints a table of the reviews items you've done today.";

        public override string ActionName => "Reviews Today";

        public override string ActionUsage => "reviews today [full]";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => @"^reviews today( full|detail(ed|s)|verbose)?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var msg = new MessageBuilder();
                var reviews = db.ReviewedItems.Where(x => x.ReviewerId == incomingChatMessage.Author.ID);
                var revCount = reviews.Count();

                if (GetRegexMatchingObject().Match(incomingChatMessage.Content).Groups[1] != null)
                {
                    msg.AppendText(reviews.ToStringTable(new[] { "Item Id", "Action", "Audit", "Completed At" },
                        x => x.Id,
                        x => x.ActionTaken,
                        x => GetFriendlyAuditResult(x.AuditPassed),
                        x => x.ReviewedOn.ToString("yyyy-MM-dd HH:mm:ss UTC")));

                    chatRoom.PostMessageOrThrow(msg);
                }
                else
                {
                    msg.AppendText($"You've reviewed {(revCount > 1 ? $"{revCount} posts today" : "a post today")}");
                    var audits = reviews.Count(x => x.AuditPassed != null);
                    if (audits > 0)
                    {
                        msg.AppendText($" ({audits} of which {(audits > 1 ? "were audits" : "was an audit")})");
                    }

                    msg.AppendText(". ");

                    // It's always possible...
                    if (revCount > 1)
                    {
                        var revDur = reviews.Max(r => r.ReviewedOn) - reviews.Min(r => r.ReviewedOn);
                        var avg = new TimeSpan(revDur.Ticks / (revCount - 1));

                        msg.AppendText("The time between your first and last review today was ");
                        msg.AppendText(revDur.ToUserFriendlyString());
                        msg.AppendText(", averaging to a review every ");
                        msg.AppendText(avg.ToUserFriendlyString());
                        msg.AppendText(".");
                    }

                    chatRoom.PostReplyOrThrow(incomingChatMessage, msg);
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
