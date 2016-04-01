using System;
using System.Linq;
using System.Reflection;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class ReviewsToday : UserCommand
    {
        public override string ActionDescription =>
            "Shows stats about your reviews completed today. Or, if requesting a full data dump (\"reviews today details\"), prints a table of the reviews items you've done today.";

        public override string ActionName => "Reviews Today";

        public override string ActionUsage => "reviews today <details>";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^reviews today(?: (details))?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var tracker = (UserTracking)typeof(Program).GetField("watcher", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                var msg = new MessageBuilder();
                var currentDate = DateTimeOffset.UtcNow;
                var revCount = tracker.WatchedUsers[incomingChatMessage.Author.ID].CompletedReviewsCount;

                var reviews = db.ReviewedItems
                    .Where(x => x.ReviewerId == incomingChatMessage.Author.ID)
                    .Where(x => x.ReviewedOn.Date == currentDate.Date)
                    .ToList();

                var endings = new[]
                {
                    "Look, I could do 40 times that in half the time you took",
                    "Slacker",
                    "I could do a lot better",
                    "Meh"
                };

                msg.AppendText($"I bet you think you're quite the hotshot for reviewing {revCount} post{(revCount == 1 ? "" : "s")}");

                var audits = reviews.Count(x => x.AuditPassed != null);
                audits += revCount - reviews.Count;
                if (audits > 0)
                {
                    msg.AppendText($" (of which {audits} {(audits > 1 ? "were audits" : "was an audit")})");
                }

                msg.AppendText($". {endings}. ");

                // if you've reviewed more than one item, give the stats about the times and speed.
                if (reviews.Count > 1)
                {
                    var durRaw = reviews.Max(r => r.ReviewedOn) - reviews.Min(r => r.ReviewedOn);
                    var durInf = new TimeSpan((durRaw.Ticks / reviews.Count) * (reviews.Count + 1));
                    var avgInf = TimeSpan.FromSeconds(durInf.TotalSeconds / reviews.Count);

                    msg.AppendText("And you know what? I don't feel like telling you any more than that.");

                    //msg.AppendText("The time between your first and last review today was ");
                    //msg.AppendText(durInf.ToUserFriendlyString());
                    //msg.AppendText(", averaging to a review every ");
                    //msg.AppendText(avgInf.ToUserFriendlyString());
                    //msg.AppendText(". ");
                }

                chatRoom.PostReplyOrThrow(incomingChatMessage, msg);

                var includeDetailsTable = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Success;

                if (includeDetailsTable)
                {
                    var detailsTable = reviews.ToStringTable(new[] { "Item Id", "Action", "Audit", "Completed At" },
                        x => x.ReviewId,
                        x => x.ActionTaken,
                        x => GetFriendlyAuditResult(x.AuditPassed),
                        x => x.ReviewedOn.ToString("yyyy-MM-dd HH:mm:ss UTC"));

                    var tableMessage = "[Here's your table, that's all you deserve, you slave driver.](http://pngimg.com/upload/table_PNG6986.png)";

                    chatRoom.PostMessageOrThrow(tableMessage);
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
