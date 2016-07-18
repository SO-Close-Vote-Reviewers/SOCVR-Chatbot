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
                var tracker = (UserTracking)typeof(Program).GetField("watcher", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

                if (!tracker.WatchedUsers.ContainsKey(incomingChatMessage.Author.ID))
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I can't produce any stats for you. Your associated tracker could not be found.");
                    return;
                }

                var msg = new MessageBuilder();
                var currentDate = DateTimeOffset.UtcNow;
                var revCount = tracker.WatchedUsers[incomingChatMessage.Author.ID].TrueReviewCount;

                var reviews = db.ReviewedItems
                    .Where(x => x.ReviewerId == incomingChatMessage.Author.ID)
                    .Where(x => x.ReviewedOn.Date == currentDate.Date)
                    .ToList();

                msg.AppendText($"You've reviewed {revCount} post{(revCount == 1 ? "" : "s")} today");

                var audits = reviews.Count(x => x.AuditPassed != null);
                audits += revCount - reviews.Count;
                if (audits > 0)
                {
                    msg.AppendText($" (of which {audits} {(audits > 1 ? "were audits" : "was an audit")})");
                }

                msg.AppendText(". ");

                // if you've reviewed more than one item, give the stats about the times and speed.
                if (reviews.Count > 1)
                {
                    var durRaw = reviews.Max(r => r.ReviewedOn) - reviews.Min(r => r.ReviewedOn);
                    var durInf = new TimeSpan((durRaw.Ticks / reviews.Count) * (reviews.Count + 1));
                    var avgInf = TimeSpan.FromSeconds(durInf.TotalSeconds / reviews.Count);

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
                    var detailsTable = reviews.ToStringTable(new[] { "Item Id", "Action", "Audit", "Completed At" },
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
