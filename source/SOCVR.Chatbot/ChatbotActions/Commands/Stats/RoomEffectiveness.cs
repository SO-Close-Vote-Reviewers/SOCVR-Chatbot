using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using ChatExchangeDotNet;
using System.Reflection;
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
                var tracker = (UserTracking)typeof(Program).GetField("watcher", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

                var totalReviews = tracker.WatchedUsers.Values.Sum(x => x.CompletedReviewsCount);

                if (totalReviews == 0)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "No one has made any reviews yet, so how do you expect me to tell you anything? Go review some stuff, or (better for me), go outside.");
                    return;
                }

                var altMessages = new []
                {
                    "Nope",
                    "Nah, don't want to",
                    "I'll do it later"
                };

                var randomNumber = new Random().Next(0, 100);

                if (randomNumber > 66)
                {
                    var reviewerCount = tracker.WatchedUsers.Values.Count(x => x.CompletedReviewsCount > 0);

                    var percentage = Math.Round(totalReviews * 1.0 / stats.ReviewsToday * 100, 2);

                    var usersPercentage = Math.Round(reviewerCount * 100D / chatRoom.PingableUsers.Count(x => x.Reputation >= 3000));

                    var message = $"{reviewerCount} members ({usersPercentage}% of this room's able reviewers) have processed {totalReviews} review items today, which accounts for {percentage}% of all CV reviews today.";

                    chatRoom.PostReplyOrThrow(incomingChatMessage, message);
                }
                else
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, altMessages.PickRandom());
                }
            }
        }
    }
}
