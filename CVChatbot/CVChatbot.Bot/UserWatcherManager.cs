using System;
using System.Collections.Generic;
using System.Linq;
using ChatExchangeDotNet;
using CVChatbot.Bot.ChatbotActions;
using CVChatbot.Bot.ChatbotActions.Commands;
using CVChatbot.Bot.Database;
using SOCVRDotNet;
using TCL.Extensions;
using User = ChatExchangeDotNet.User;

namespace CVChatbot.Bot
{
    public class UserWatcherManager : IDisposable
    {
        private readonly DatabaseAccessor dbAccessor;
        private readonly InstallationSettings initSettings;
        private readonly Room room;
        private bool dispose;

        public List<UserWatcher> Watchers { get; private set; }



        public UserWatcherManager(ref Room chatRoom, InstallationSettings settings)
        {
            if (chatRoom == null) { throw new ArgumentNullException("chatRoom"); }
            if (settings == null) { throw new ArgumentNullException("settings"); }

            room = chatRoom;
            initSettings = settings;
            Watchers = new List<UserWatcher>();
            dbAccessor = new DatabaseAccessor(settings.DatabaseConnectionString);
            var pingable = chatRoom.GetPingableUsers();

            foreach (var user in pingable)
            {
                if (dbAccessor.GetRegisteredUserByChatProfileId(user.ID) == null) { continue; }

                var watcher = InitialiseWatcher(user.ID);
                Watchers.Add(watcher);
            }

            // Look out for registered members that haven't joined in a while.
            chatRoom.EventManager.ConnectListener(EventType.UserEntered, new Action<User>(u => HandleNewUser(u.ID)));
        }

        ~UserWatcherManager()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) { return; }
            dispose = true;

            foreach (var watcher in Watchers)
            {
                watcher.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void AddUser(int userID)
        {
            HandleNewUser(userID);
        }

        public void AddUser(User user)
        {
            if (user == null) { throw new ArgumentNullException("user"); }

            AddUser(user.ID);
        }



        private UserWatcher InitialiseWatcher(int userID)
        {
            var watcher = new UserWatcher(userID)
            {
                // TODO: You may want to set these.
                // AuditFailureFactor = ?,
                // IdleFactor = ?
            };

            watcher.EventManager.ConnectListener(UserEventType.StartedReviewing,
                new Action(() => HandleStartedReviewing(watcher)));

            watcher.EventManager.ConnectListener(UserEventType.FinishedReviewing,
                new Action<DateTime, DateTime, List<ReviewItem>>((start, end, rs) =>
                HandleFinishedReviewing(watcher, start, end, rs)));

            watcher.EventManager.ConnectListener(UserEventType.FailedAudit,
                new Action<ReviewItem>(r => HandleAuditFailed(watcher, r)));

            watcher.EventManager.ConnectListener(UserEventType.PassedAudit,
                new Action<ReviewItem>(r => HandleAuditPassed(watcher, r)));

            watcher.EventManager.ConnectListener(UserEventType.ReviewedTag,
                new Action<KeyValuePair<string, float>, DateTime>((tagKv, timestamp) => HandleReviewedTag(watcher, tagKv, timestamp)));

            watcher.EventManager.ConnectListener(UserEventType.InternalException,
                new Action<Exception>(ex => HandleException(watcher, ex)));

            return watcher;
        }

        private void HandleNewUser(int userID)
        {
            if (dbAccessor.GetRegisteredUserByChatProfileId(userID) == null ||
                Watchers.Any(w => w.UserID == userID))
            {
                return;
            }

            var watcher = InitialiseWatcher(userID);
            Watchers.Add(watcher);
        }

        private void HandleStartedReviewing(UserWatcher watcher)
        {
            var chatUser = room.GetUser(watcher.UserID);
            var numberOfClosedSessions = dbAccessor.EndAnyOpenSessions(watcher.UserID);

            // Now record the new session.
            dbAccessor.StartReviewSession(watcher.UserID);

            var outMessage = "I've noticed you've started reviewing. I'll make a new session for you. Good luck!";
            outMessage = "@" + chatUser.Name.Replace(" ", "") + " " + outMessage;

            // If there was a closed session.
            if (numberOfClosedSessions > 0)
            {
                // Append a message saying how many there were.
                outMessage += " **Note:** You had {0} open {1}. I have closed {2}.".FormatInline(
                    numberOfClosedSessions,
                    numberOfClosedSessions > 1
                        ? "sessions"
                        : "session",
                    numberOfClosedSessions > 1
                        ? "them"
                        : "it");
            }

            room.PostMessageOrThrow(outMessage);
        }

        private void HandleFinishedReviewing(UserWatcher watcher, DateTime startTime, DateTime endTime, List<ReviewItem> reviews)
        {
            // Find the latest session by that user.
            var latestSession = dbAccessor.GetLatestOpenSessionForUser(watcher.UserID);
            var ping = "@" + room.GetUser(watcher.UserID).Name.Replace(" ", "") + " ";
            var msg = "";

            // First, check if there is a session.
            if (latestSession == null)
            {
                msg = ping + "I don't seem to have the start of your review session on record. " +
                      "I might have not been running when you started, or some error happened.";
                room.PostMessageOrThrow(msg);
                return;
            }

            // Check if session is greater than [MAX_REVIEW_TIME].
            var maxReviewTimeHours = initSettings.MaxReviewLengthHours;

            var timeThreshold = DateTimeOffset.Now.AddHours(-maxReviewTimeHours);

            if (latestSession.SessionStart < timeThreshold)
            {
                var timeDelta = DateTimeOffset.Now - latestSession.SessionStart;

                msg = ping + "Your last uncompleted review session was {0} ago. " +
                      "Because it has exceeded my threshold ({1} hours), " +
                      "I can't mark that session with this information. "
                      .FormatInline(timeDelta.ToUserFriendlyString(), maxReviewTimeHours) +
                      "Use the command '{0}' to forcefully end that session."
                      .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<EndSession>());

                room.PostMessageOrThrow(msg);
                return;
            }

            // It's all good, mark the info as done.
            dbAccessor.EndReviewSession(latestSession.Id, reviews.Count);

            msg = ping +
                  "Thanks for reviewing! To see more information use the command `{0}`."
                  .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>());
            room.PostMessageOrThrow(msg);
        }

        private void HandleAuditPassed(UserWatcher watcher, ReviewItem audit)
        {
            dbAccessor.InsertCompletedAuditEntry(watcher.UserID, audit.Tags[0]);
        }

        private void HandleAuditFailed(UserWatcher watcher, ReviewItem audit)
        {
            // Do something...
        }

        private void HandleReviewedTag(UserWatcher watcher, KeyValuePair<string, float> tagKv, DateTime completedTime)
        {
            dbAccessor.InsertNoItemsInFilterRecord(watcher.UserID, tagKv.Key);
        }

        private void HandleException(UserWatcher watcher, Exception ex)
        {
            room.PostMessageOrThrow("error happened!\n" + ex.FullErrorMessage(Environment.NewLine));
        }
    }
}
