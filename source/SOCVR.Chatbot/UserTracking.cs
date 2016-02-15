using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvsAnLib;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using SOCVRDotNet;
using TCL.Extensions;
using EventType = SOCVRDotNet.EventType;
using User = SOCVRDotNet.User;

namespace SOCVR.Chatbot
{
    internal class UserTracking : IDisposable
    {
        private readonly ManualResetEvent dbWatcherMre = new ManualResetEvent(false);
        private readonly Room room;
        private bool dispose;

        public ConcurrentDictionary<int, User> WatchedUsers { get; private set; }



        public UserTracking(ref Room chatRoom)
        {
            if (chatRoom == null) throw new ArgumentNullException("chatRoom");

            room = chatRoom;
            WatchedUsers = new ConcurrentDictionary<int, User>();

            InitialiseWatcher();

            Task.Run(() => WatchDB());
        }

        ~UserTracking()
        {
            Dispose();
        }



        public static UserReviewedItem GetUserReviewedItem(int reviewID, int profileID)
        {
            var fkey = UserDataFetcher.GetFkey();
            var rev = new ReviewItem(reviewID, fkey);
            var res = rev.Results.First(x => x.UserID == profileID);

            return new UserReviewedItem
            {
                ActionTaken = (ReviewItemAction)(int)res.Action,
                AuditPassed = rev.AuditPassed,
                PrimaryTag = rev.Tags[0].ToLowerInvariant(),
                ReviewedOn = res.Timestamp,
                ReviewerId = profileID,
                ReviewId = reviewID
            };
        }

        public void Dispose()
        {
            if (dispose) return;
            dispose = true;

            dbWatcherMre.Set();

            foreach (var id in WatchedUsers.Keys)
            {
                WatchedUsers[id].Dispose();
            }

            GC.SuppressFinalize(this);
        }



        private void InitialiseWatcher()
        {
            using (var db = new DatabaseContext())
            {
                var users = db.UserPermissions
                    .Where(x => x.PermissionGroup == PermissionGroup.Reviewer &&
                                x.User.OptInToReviewTracking);

                foreach (var user in users)
                {
                    AddUser(user.UserId);
                }
            }

            RequestThrottler.RequestThroughputMin = 30;
        }

        private void WatchDB()
        {
            while (!dispose)
            {
                using (var db = new DatabaseContext())
                {
                    var curUsers = db.UserPermissions
                        .Where(x => x.PermissionGroup == PermissionGroup.Reviewer &&
                                    x.User.OptInToReviewTracking);

                    foreach (var user in curUsers)
                    {
                        if (!WatchedUsers.ContainsKey(user.UserId))
                        {
                            AddUser(user.UserId);
                        }
                    }

                    var usersToRemove = new List<int>();
                    foreach (var id in WatchedUsers.Keys)
                    {
                        if (!curUsers.Any(x => x.UserId == id))
                        {
                            usersToRemove.Add(id);
                        }
                    }
                    foreach (var id in usersToRemove)
                    {
                        RemoveUser(id);
                    }

                    dbWatcherMre.WaitOne(TimeSpan.FromSeconds(3));
                }
            }
        }

        private void AddUser(int userID)
        {
            if (WatchedUsers.ContainsKey(userID)) return;

            WatchedUsers[userID] = new User(userID);
            HookUpUserEvents(userID);
        }

        private void RemoveUser(int userID)
        {
            if (!WatchedUsers.ContainsKey(userID)) return;

            User temp;
            WatchedUsers.TryRemove(userID, out temp);
            temp.Dispose();
        }

        private void HookUpUserEvents(int id)
        {
            WatchedUsers[id].EventManager.ConnectListener(EventType.ItemReviewed,
                new Action<ReviewItem>(r => SaveReview(r, id)));

            WatchedUsers[id].EventManager.ConnectListener(EventType.ReviewingStarted,
                new Action(() => HandleReviewingStarted(WatchedUsers[id])));

            WatchedUsers[id].EventManager.ConnectListener(EventType.ReviewingCompleted,
                new Action<HashSet<ReviewItem>>(revs => HandleReviewingCompleted(WatchedUsers[id], revs)));

            WatchedUsers[id].EventManager.ConnectListener(EventType.AuditPassed,
                new Action<ReviewItem>(r => HandleAuditPassed(WatchedUsers[id], r)));

            WatchedUsers[id].EventManager.ConnectListener(EventType.InternalException,
                new Action<Exception>(ex => HandleException(ex)));
        }

        private void HandleReviewingStarted(User user)
        {
            var msg = new MessageBuilder();

            if (room.GetCurrentUsers().Any(x => x.ID == user.ID))
            {
                msg.AppendPing(room.GetUser(user.ID));
                msg.AppendText("I've noticed you've started reviewing! I'll update your session record.");
            }
            else
            {
                msg.AppendText($"{room.GetUser(user.ID).GetChatFriendlyUsername()} has started reviewing!");
            }

            room.PostMessageLight(msg);
        }

        private void HandleReviewingCompleted(User user, HashSet<ReviewItem> reviews)
        {
            var revCount = user.CompletedReviewsCount;
            var userInRoom = room.GetCurrentUsers().Any(x => x.ID == user.ID);
            var chatUser = room.GetUser(user.ID);
            var shortName = chatUser.GetChatFriendlyUsername();
            var msg = new MessageBuilder();

            if (userInRoom)
            {
                msg.AppendPing(chatUser);
            }

            var posts = reviews.Count > 1 ? $"{revCount} posts today" : "a post today";
            msg.AppendText($"{(userInRoom ? "You've" : shortName)} reviewed {posts}");

            var audits = reviews.Count(x => x.AuditPassed != null);
            if (audits > 0)
            {
                msg.AppendText($" ({audits} of which {(audits > 1 ? "were audits" : "was an audit")})");
            }

            msg.AppendText(userInRoom ? ", thanks! " : "! ");

            // It's always possible...
            if (reviews.Count > 1)
            {
                var revRes = reviews.Select(r => r.Results.First(rr => rr.UserID == user.ID));
                var durRaw = revRes.Max(r => r.Timestamp) - revRes.Min(r => r.Timestamp);
                var durInf = new TimeSpan((durRaw.Ticks / revCount) * (revCount + 1));
                var avgInf = TimeSpan.FromSeconds(durInf.TotalSeconds / revCount);
                var pronounOrName = userInRoom ? "your" : shortName + "'s";

                msg.AppendText($"The time between {pronounOrName} first and last review today was ");
                msg.AppendText(durInf.ToUserFriendlyString());
                msg.AppendText(", averaging to a review every ");
                msg.AppendText(avgInf.ToUserFriendlyString());
                msg.AppendText(".");
            }

            room.PostMessageLight(msg);
        }

        private void HandleAuditPassed(User user, ReviewItem audit)
        {
            //TODO: calc which tag is most relevant and use that instead
            // of just picking the first tag on the post.

            var tooltip = "";
            foreach (var t in audit.Tags)
            {
                tooltip += $"[{t}] ";
            }
            tooltip += $"audit passed at {audit.Results.First().Timestamp.ToString("HH:mm:ss")} UTC.";

            var message = new MessageBuilder();
            var tag = audit.Tags[0].ToLowerInvariant();

            message.AppendText(room.GetUser(user.ID).GetChatFriendlyUsername());
            message.AppendText(" passed ");
            message.AppendText(AvsAn.Query(tag).Article + " ");
            message.AppendText(tag, TextFormattingOptions.Tag, WhiteSpace.Space);
            message.AppendLink("audit", "http://stackoverflow.com/review/close/" + audit.ID, tooltip);
            message.AppendText("!");

            room.PostMessageOrThrow(message.Message);
        }

        private void SaveReview(ReviewItem rev, int userID)
        {
            Program.WriteToConsole($"Saving ReviewItem {rev.ID} for user {userID}.");
            var res = rev.Results.First(x => x.UserID == userID);
            using (var db = new DatabaseContext())
            {
                db.EnsureUserExists(res.UserID);

                db.ReviewedItems.Add(new UserReviewedItem
                {
                    ActionTaken = (ReviewItemAction)(int)res.Action,
                    AuditPassed = rev.AuditPassed,
                    PrimaryTag = rev.Tags[0].ToLowerInvariant(),
                    ReviewedOn = res.Timestamp,
                    ReviewerId = res.UserID,
                    ReviewId = rev.ID
                });
                db.SaveChanges();
            }
        }

        private void HandleException(Exception ex)
        {
            var headerLine = "An error happened in UserTracking (cc @Sam)";
            var errorMessage = "    " + ex.FullErrorMessage(Environment.NewLine + "    ");
            var stackTraceMessage = ex.GetAllStackTraces();

            var detailsLine = errorMessage + Environment.NewLine +
                "    ----" + Environment.NewLine +
                stackTraceMessage;

            room.PostMessageOrThrow(headerLine);
            room.PostMessageOrThrow(detailsLine);
        }
    }
}