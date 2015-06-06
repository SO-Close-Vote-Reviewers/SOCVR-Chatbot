using System;
using System.Collections.Generic;
using SOCVRDotNet;
using CVChatbot.Bot.Database;
using ChatExchangeDotNet;
using TCL.Extensions;
using CVChatbot.Bot.ChatbotActions;
using CVChatbot.Bot.ChatbotActions.Commands;

namespace CVChatbot.Bot
{
    public class UserWatcherManager : IDisposable
    {
        private readonly List<UserWatcher> watchers;
        private readonly DatabaseAccessor dbAccessor;
        private readonly InstallationSettings initSettings;
        private readonly Room room;
        private bool dispose;



        public UserWatcherManager(Room chatRoom, InstallationSettings settings)
        {
            if (chatRoom == null) { throw new ArgumentNullException("chatRoom"); }
            if (settings == null) { throw new ArgumentNullException("settings"); }

            room = chatRoom;
            initSettings = settings;
            watchers = new List<UserWatcher>();
            dbAccessor = new DatabaseAccessor(settings.DatabaseConnectionString);
            var pingable = chatRoom.GetPingableUsers();

            foreach (var user in pingable)
            {
                if (dbAccessor.GetRegisteredUserByChatProfileId(user.ID) == null) { continue; }
                var watcher = new UserWatcher(user.ID);

                // TODO: You may want to set these...
                watcher.IdleTimeout = TimeSpan.FromMinutes(1);
                watcher.AuditFailureTimeout = TimeSpan.FromSeconds(30);

                watcher.EventManager.ConnectListener(UserEventType.StartedReviewing,
                    new Action(() => HandleStartedReviewing(watcher)));

                watcher.EventManager.ConnectListener(UserEventType.FinishedReviewing,
                    new Action<DateTime, DateTime, List<ReviewItem>>((start, end, rs) =>
                    HandleFinishedReviewing(watcher, start, end, rs)));

                watcher.EventManager.ConnectListener(UserEventType.FailedAudit,
                    new Action<ReviewItem>(r => HandleAuditFailed(watcher, r)));

                watcher.EventManager.ConnectListener(UserEventType.PassedAudit,
                    new Action<ReviewItem>(r => HandleAuditPassed(watcher, r)));

                watcher.EventManager.ConnectListener(UserEventType.InternalException,
                    new Action<Exception>(ex => HandleException(watcher, ex)));

                watchers.Add(watcher);
            }
        }

        ~UserWatcherManager()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) { return; }
            dispose = true;

            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }

            GC.SuppressFinalize(this);
        }



        private void HandleStartedReviewing(UserWatcher watcher)
        {
            var chatUser = room.GetUser(watcher.UserID);
            var numberOfClosedSessions = dbAccessor.EndAnyOpenSessions(watcher.UserID);

            // Now record the new session.
            dbAccessor.StartReviewSession(watcher.UserID);

            var replyMessages = new List<string>()
            {
                "Good luck!",
                "Happy reviewing!",
                "Don't get lost in the queue!",
                "Watch out for audits!",
                "May the Vote be with you!",
                "May Shog9's Will be done.",
                "By the power of the Vote! Review!"
            };

            var outMessage = replyMessages.PickRandom();
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

            // First, check if there is a session.
            if (latestSession == null)
            {
                var msg = "@" + room.GetUser(watcher.UserID).Name.Replace(" ", "") +
                    " I don't seem to have the start of your review session on record. " +
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

                var message = "@" + room.GetUser(watcher.UserID).Name.Replace(" ", "") +
                    " Your last uncompleted review session was {0} ago. " +
                    "Because it has exceeded my threshold ({1} hours), " +
                    "I can't mark that session with this information. "
                    .FormatInline(timeDelta.ToUserFriendlyString(), maxReviewTimeHours) +
                    "Use the command '{0}' to forcefully end that session."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<EndSession>());

                room.PostMessageOrThrow(message);
                return;
            }

            // It's all good, mark the info as done.
            dbAccessor.EndReviewSession(latestSession.Id, reviews.Count);
        }

        private void HandleAuditFailed(UserWatcher watcher, ReviewItem audit)
        {
            // Do something...
        }

        private void HandleAuditPassed(UserWatcher watcher, ReviewItem audit)
        {
            dbAccessor.InsertCompletedAuditEntry(watcher.UserID, audit.Tags[0]);
        }

        private void HandleException(UserWatcher watcher, Exception ex)
        {
            room.PostMessageOrThrow("error happened!\n" + ex.FullErrorMessage(Environment.NewLine));
        }
    }
}
