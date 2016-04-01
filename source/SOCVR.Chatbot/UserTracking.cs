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
            //TODO: We may need to tweak this later (currently set to 1 by default).
            //RequestThrottler.ThrottleFactor = 1.5F;

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

            // Screw long boot times, let's init this in the bg.
            WatchedUsers[userID] = new User(userID, true);
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
            var msg = new MessageBuilder(MultiLineMessageType.None, false);

            if (room.CurrentUsers.Any(x => x.ID == user.ID))
            {
                msg.AppendPing(room.GetUser(user.ID));

                var messages = new[]
                {
                    "Oh great, another person reviewing. Now I've got _more_ too keep track of. Thanks for the increased workload :(",
                    "You started reviewing, but what's the chances I'll 'forget' to keep track?",
                    "That's a nice review session you've started, it would be a _shame_ if something happened to it.",
                    "You know, everyone else that's reviewed today, I'll keep track of. There's something about you that makes me not care. Don't expect accurate results in the end."
                };

                msg.AppendText(messages.PickRandom());
            }
            else
            {
                msg.AppendText($"{room.GetUser(user.ID)} has started reviewing (twirls finger...)");
            }

            room.PostMessageLight(msg);
        }

        private void HandleReviewingCompleted(User user, HashSet<ReviewItem> reviews)
        {
            var revCount = user.CompletedReviewsCount;
            var userInRoom = room.CurrentUsers.Any(x => x.ID == user.ID);
            var chatUser = room.GetUser(user.ID);
            var msg = new MessageBuilder();

            if (userInRoom)
            {
                msg.AppendPing(chatUser);
            }

            msg.AppendText(" fished reviewing. I'm too lazy to get your stats now. Ask me later...");

            room.PostMessageLight(msg);
        }

        private void HandleAuditPassed(User user, ReviewItem audit)
        {
            //TODO: calc which tag is most relevant and use that instead
            // of just picking the first tag on the post.

            var userName = room.GetUser(user.ID).Name;
            var tag = audit.Tags[0].ToLowerInvariant();
            var aOrAn = AvsAn.Query(tag).Article;
            var tagText = $"[tag:{tag}]";

            var messages = new[]
            {
                $"{userName} just passed {aOrAn} {tagText} audit. Aren't you the king of reviews?",
                $"{userName} passed {aOrAn} {tagText} audit. I bet the next one you'll fail.",
                $"{userName} passed {aOrAn} {tagText} audit. Don't let it go to your head.",
            };

            room.PostMessageOrThrow(messages.PickRandom());
        }

        private void SaveReview(ReviewItem rev, int userID)
        {
            Program.WriteToConsole($"Saving ReviewItem {rev.ID} for user {userID}.");
            var res = rev.Results.First(x => x.UserID == userID);
            using (var db = new DatabaseContext())
            {
                db.EnsureUserExists(res.UserID);

                var reviewAlreadyExists = db.ReviewedItems
                    .Where(x => x.ReviewId == rev.ID)
                    .Where(x => x.ReviewerId == res.UserID)
                    .Any();

                if (reviewAlreadyExists)
                {
                    return;
                }

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