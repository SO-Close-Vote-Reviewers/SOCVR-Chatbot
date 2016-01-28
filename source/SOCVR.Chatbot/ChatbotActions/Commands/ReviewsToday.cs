using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class ReviewsToday : UserCommand
    {
        public override string ActionDescription =>
            "Shows stats about your reviews completed today. Or, if requesting a full data dump (`review today details`), prints a table of the reviews items you've done today.";

        public override string ActionName => "Reviews Today";

        public override string ActionUsage => "reviews today";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override string RegexMatchingPattern => @"^reviews today (full|detail(ed|s)|verbose)?$";


        /// <summary>
        /// Warning, completely untested code ahead.
        /// </summary>
        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var msgText = "";
                var reviews = db.ReviewedItems.Where(x => x.ReviewerId == incomingChatMessage.Author.ID);
                var revCount = reviews.Count();

                if (GetRegexMatchingObject().Match(incomingChatMessage.Content).Groups[1] != null)
                {
                    msgText = reviews.ToStringTable(new[] { "Item Id", "Action", "Audit", "Completed At" },
                        x => x.Id,
                        x => x.ActionTaken,
                        x => GetFriendlyAuditResult(x.AuditPassed),
                        x => x.ReviewedOn.ToString("yyyy-MM-dd HH:mm:ss UTC"));

                    chatRoom.PostMessageOrThrow(msgText);
                }
                else
                {
                    msgText = $"You've reviewed {(revCount > 1 ? $"{revCount} posts today" : "a post today")}";
                    var audits = reviews.Count(x => x.AuditPassed != null);
                    if (audits > 0)
                    {
                        msgText += $" ({audits} of which {(audits > 1 ? "were audits" : "was an audit")})";
                    }

                    msgText += ". ";

                    // It's always possible...
                    if (revCount > 1)
                    {
                        var revDur = reviews.Max(r => r.ReviewedOn) - reviews.Min(r => r.ReviewedOn);
                        var avg = new TimeSpan(revDur.Ticks / (revCount - 1));

                        msgText += "The time between your first and last review today was ";
                        msgText += revDur.Hours > 0 ? $"{revDur.Hours} hour{(revDur.Hours > 1 ? "s" : "")} and " : "";
                        msgText += $"{revDur.Minutes} minute{(revDur.Minutes > 1 ? "s" : "")}";
                        msgText += ", averaging to a review every ";

                        if (avg.Hours > 0)
                        {
                            msgText += avg.Hours > 0 ? $"{avg.Hours} hour{(avg.Hours > 1 ? "s" : "")} and " : "";
                            msgText += $"{avg.Minutes} minute{(avg.Minutes > 1 ? "s" : "")}.";
                        }
                        else
                        {
                            msgText += avg.Minutes > 0 ? $"{avg.Minutes} minute{(avg.Minutes > 1 ? "s" : "")} and " : "";
                            msgText += $"{avg.Seconds} second{(avg.Seconds > 1 ? "s" : "")}.";
                        }
                    }

                    chatRoom.PostReplyOrThrow(incomingChatMessage, msgText);
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
