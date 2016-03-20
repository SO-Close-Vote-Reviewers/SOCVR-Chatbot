using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Stats
{
    internal class RoomEffectiveness : UserCommand
    {
        public override string ActionDescription => "Shows stats about how effective the room is at processing close vote review items.";

        public override string ActionName => "Room Effectiveness";

        public override string ActionUsage => "room effectiveness";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^room effectiveness$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                //get the number of reviews done today by the entire site
                var sa = new CloseQueueStatsAccessor();
                var stats = sa.GetOverallQueueStats();

                var reviewsInDbToday = db.ReviewedItems
                    .Where(x => x.ReviewedOn.Date == DateTimeOffset.UtcNow.Date)
                    .ToList();

                if (reviewsInDbToday.Count == 0)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I don't have enough data to produce those stats.");
                    return;
                }

                var reviewerCount = reviewsInDbToday
                    .Select(x => x.ReviewerId)
                    .Distinct()
                    .Count();

                var reviewItemCount = reviewsInDbToday.Count;

                var percentage = Math.Round(reviewItemCount * 1.0 / stats.ReviewsToday * 100, 2);

                var usersPercentage = Math.Round((double)reviewerCount / chatRoom.PingableUsers.Count(x => x.Reputation >= 3000));

                var message = $"{reviewerCount} members ({usersPercentage}% of this room's able reviewers) have processed {reviewItemCount} review items today, which accounts for {percentage}% of all CV reviews today.";

                chatRoom.PostReplyOrThrow(incomingChatMessage, message);
            }
        }
    }
}
