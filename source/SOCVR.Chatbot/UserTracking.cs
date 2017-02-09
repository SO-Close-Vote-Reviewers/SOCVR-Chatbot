using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvsAnLib;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using CVQMonitor;
using TCL.Extensions;
using User = CVQMonitor.User;

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
            var rev = new Review(reviewID, profileID);

            return new UserReviewedItem
            {
                ActionTaken = (ReviewItemAction)(int)rev.Action,
                AuditPassed = rev.AuditPassed,
                PrimaryTag = rev.Tags[0].ToLowerInvariant(),
                ReviewedOn = rev.Timestamp,
                ReviewerId = profileID,
                ReviewId = reviewID
            };
        }

        public void Dispose()
        {
            if (dispose) return;
            dispose = true;

            dbWatcherMre.Set();
            dbWatcherMre.Dispose();

            foreach (var id in WatchedUsers.Keys)
            {
                ((IDisposable)WatchedUsers[id]).Dispose();
            }

            GC.SuppressFinalize(this);
        }



        private void InitialiseWatcher()
        {
            //TODO: We may need to tweak this later.
            RequestScheduler.RequestsPerMinute = 45;

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
            ((IDisposable)temp).Dispose();
        }

        private void HookUpUserEvents(int id)
        {
            WatchedUsers[id].ItemReviewed += (o, e) =>
            {
                SaveReview(e.Item2);
                if (e.Item2.AuditPassed == true)
                {
                    HandleAuditPassed(e.Item1, e.Item2);
                }
            };

            WatchedUsers[id].ReviewingStarted += (o, e) => HandleReviewingStarted(e);

            WatchedUsers[id].ReviewLimitReached += (o, e) =>
            {
                HandleReviewingCompleted(e, e.ReviewsToday.ToList());
            };
        }

        private void HandleReviewingStarted(User user)
        {
            var msg = new MessageBuilder();

            if (room.CurrentUsers.Any(x => x.ID == user.ID))
            {
                msg.AppendPing(room.GetUser(user.ID));
                msg.AppendText("I've noticed you've started reviewing! I'll update your session record.");
            }
            else
            {
                msg.AppendText($"{room.GetUser(user.ID)} has started reviewing!");
            }

            room.PostMessageLight(msg);
        }

        private void HandleReviewingCompleted(User user, ICollection<Review> reviews)
        {
            var revCount = user.TrueReviewCount;
            var userInRoom = room.CurrentUsers.Any(x => x.ID == user.ID);
            var chatUser = room.GetUser(user.ID);
            var msg = new MessageBuilder();

            if (userInRoom)
            {
                msg.AppendPing(chatUser);
            }

            var posts = reviews.Count > 1 ? $"{revCount} posts today" : "a post today";
            msg.AppendText($"{(userInRoom ? "You've" : chatUser.Name)} reviewed {posts}");

            var audits = reviews.Count(x => x.AuditPassed != null) + (revCount - reviews.Count);
            if (audits > 0)
            {
                msg.AppendText($" (of which {audits} {(audits > 1 ? "were audits" : "was an audit")})");
            }

            msg.AppendText(userInRoom ? ", thanks! " : "! ");

            // It's always possible...
            if (reviews.Count > 1)
            {
                var durRaw = reviews.Max(r => r.Timestamp) - reviews.Min(r => r.Timestamp);
                var durInf = new TimeSpan((durRaw.Ticks / revCount) * (revCount + 1));
                var avgInf = TimeSpan.FromSeconds(durInf.TotalSeconds / revCount);
                var pronounOrName = userInRoom ? "your" : chatUser.Name + "'s";

                msg.AppendText($"The time between {pronounOrName} first and last review today was ");
                msg.AppendText(durInf.ToUserFriendlyString());
                msg.AppendText(", averaging to a review every ");
                msg.AppendText(avgInf.ToUserFriendlyString());
                msg.AppendText(".");
            }

            room.PostMessageLight(msg);
        }

        private void HandleAuditPassed(User user, Review audit)
        {
            //TODO: calc which tag is most relevant and use that instead
            // of just picking the first tag on the post.

            var tooltip = "";
            foreach (var t in audit.Tags)
            {
                tooltip += $"[{t}] ";
            }
            tooltip += $"audit passed at {audit.Timestamp.ToString("HH:mm:ss")} UTC.";

            var message = new MessageBuilder();
            var tag = audit.Tags[0].ToLowerInvariant();

            message.AppendText(room.GetUser(user.ID).Name);
            message.AppendText(" passed ");
            message.AppendText(AvsAn.Query(tag).Article + " ");
            message.AppendText(tag, TextFormattingOptions.Tag, WhiteSpace.Space);
            message.AppendLink("audit", "http://stackoverflow.com/review/close/" + audit.ID, tooltip);
            message.AppendText("!");

            room.PostMessageOrThrow(message.Message);
        }

        private void SaveReview(Review rev)
        {
            Program.WriteToConsole($"Saving ReviewItem {rev.ID} for {rev.ReviewerName} ({rev.ReviewerID}).");
            using (var db = new DatabaseContext())
            {
                db.EnsureUserExists(rev.ReviewerID);

                var reviewAlreadyExists = db.ReviewedItems
                    .Where(x => x.ReviewId == rev.ID && x.ReviewerId == rev.ReviewerID)
                    .Any();

                if (reviewAlreadyExists) return;

                db.ReviewedItems.Add(new UserReviewedItem
                {
                    ActionTaken = (ReviewItemAction)(int)rev.Action,
                    AuditPassed = rev.AuditPassed,
                    PrimaryTag = rev.Tags[0].ToLowerInvariant(),
                    ReviewedOn = rev.Timestamp,
                    ReviewerId = rev.ReviewerID,
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