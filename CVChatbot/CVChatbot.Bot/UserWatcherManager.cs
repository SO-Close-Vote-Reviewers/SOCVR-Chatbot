/*
 * ChatExchange.Net. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly ConcurrentDictionary<Message, List<string>> tagReviewedConfirmationQueue;
        private readonly Regex yesRegex;
        private readonly Regex noRegex;
        private bool dispose;

        public List<UserWatcher> Watchers { get; private set; }



        public UserWatcherManager(ref Room chatRoom, InstallationSettings settings)
        {
            if (chatRoom == null) { throw new ArgumentNullException("chatRoom"); }
            if (settings == null) { throw new ArgumentNullException("settings"); }

            room = chatRoom;
            initSettings = settings;
            tagReviewedConfirmationQueue = new ConcurrentDictionary<Message, List<string>>();
            yesRegex = new Regex(@"(?i)\by[ue][aps]h?\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            noRegex = new Regex(@"(?i)\bno*(pe|t)?\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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
            chatRoom.EventManager.ConnectListener(EventType.UserEntered, new Action<User>(u => AddNewUser(u.ID)));

            // Listen out for tracking new users.
            chatRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleTrackUserCommand(m)));

            // Listen for tag confirmations.
            chatRoom.EventManager.ConnectListener(EventType.MessageReply, new Action<Message, Message>((parent, reply) => HandleCurrentTagsChangedConfirmation(reply)));
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
            AddNewUser(userID, false);
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
                //TagTrackingEnabled = ?
                //AuditFailureFactor = ?,
                //PollInterval = ?,
                //IdleFactor = ?
            };

            watcher.EventManager.ConnectListener(UserEventType.ReviewingStarted,
                new Action(() => HandleReviewingStarted(watcher)));

            watcher.EventManager.ConnectListener(UserEventType.ReviewingFinished,
                new Action<DateTime, DateTime, List<ReviewItem>>((start, end, rs) =>
                HandleReviewingFinished(watcher, start, end, rs)));

            watcher.EventManager.ConnectListener(UserEventType.AuditFailed,
                new Action<ReviewItem>(r => HandleAuditFailed(watcher, r)));

            watcher.EventManager.ConnectListener(UserEventType.AuditPassed,
                new Action<ReviewItem>(r => HandleAuditPassed(watcher, r)));

            watcher.EventManager.ConnectListener(UserEventType.CurrentTagsChanged,
                new Action<List<string>>((oldTags) => HandleCurrentTagsChanged(watcher, oldTags)));

            watcher.EventManager.ConnectListener(UserEventType.InternalException,
                new Action<Exception>(ex => HandleException(watcher, ex)));

            return watcher;
        }

        private void HandleTrackUserCommand(Message m)
        {
            var action = ChatbotActionRegister.AllChatActions.First(a => a is TrackUser);
            if (!action.DoesChatMessageActiveAction(m, true)) { return; }

            var user = dbAccessor.GetRegisteredUserByChatProfileId(m.Author.ID);
            if (user == null || !user.IsOwner) { return; }

            var newUserId = 0;
            var sucess = int.TryParse(new string(m.Content.Where(char.IsDigit).ToArray()), out newUserId);
            if (!sucess) { return; }

            AddNewUser(newUserId, false);
        }

        private void AddNewUser(int userID, bool checkDb = true)
        {
            if ((dbAccessor.GetRegisteredUserByChatProfileId(userID) == null && checkDb) ||
                Watchers.Any(w => w.UserID == userID))
            {
                return;
            }

            var watcher = InitialiseWatcher(userID);
            Watchers.Add(watcher);
        }

        private void HandleReviewingStarted(UserWatcher watcher)
        {
            var chatUser = room.GetUser(watcher.UserID);
            var numberOfClosedSessions = dbAccessor.EndAnyOpenSessions(watcher.UserID);

            // Now record the new session.
            dbAccessor.StartReviewSession(watcher.UserID);

            var message = new MessageBuilder();

            message.AppendPing(chatUser);
            message.AppendText("I've noticed you've started reviewing. I'll make a new session for you. Good luck! ");

            // If there was a closed session.
            if (numberOfClosedSessions > 0)
            {
                // Append a message saying how many there were.
                message.AppendText("Note: ", TextFormattingOptions.Bold);
                message.AppendText("You had {0} open {1}. I have closed {2}.".FormatInline(
                    numberOfClosedSessions,
                    numberOfClosedSessions > 1
                        ? "sessions"
                        : "session",
                    numberOfClosedSessions > 1
                        ? "them"
                        : "it"));
            }

            room.PostMessageOrThrow(message.Message);
        }

        private void HandleReviewingFinished(UserWatcher watcher, DateTime startTime, DateTime endTime, List<ReviewItem> reviews)
        {
            // Find the latest session by that user.
            var latestSession = dbAccessor.GetLatestOpenSessionForUser(watcher.UserID);
            var message = new MessageBuilder();

            message.AppendPing(room.GetUser(watcher.UserID));

            // First, check if there is a session.
            if (latestSession == null)
            {
                message.AppendText("I don't seem to have the start of your review session on record. ");
                message.AppendText("I might have not been running when you started, or some error happened.");
                room.PostMessageOrThrow(message.Message);
                return;
            }

            // Check if session is greater than [MAX_REVIEW_TIME].
            var maxReviewTimeHours = initSettings.MaxReviewLengthHours;

            var timeThreshold = DateTimeOffset.Now.AddHours(-maxReviewTimeHours);

            if (latestSession.SessionStart < timeThreshold)
            {
                var timeDelta = DateTimeOffset.Now - latestSession.SessionStart;

                message.AppendText("Your last uncompleted review session was {0} ago. " +
                      "Because it has exceeded my threshold ({1} hours), " +
                      "I can't mark that session with this information. "
                      .FormatInline(timeDelta.ToUserFriendlyString(), maxReviewTimeHours) +
                      "Use the command '{0}' to forcefully end that session."
                      .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<EndSession>()));

                room.PostMessageOrThrow(message.Message);
                return;
            }

            // It's all good, mark the info as done.
            dbAccessor.EndReviewSession(latestSession.Id, reviews.Count, endTime);

            message.AppendText("Thanks for reviewing! To see more information use the command ");
            message.AppendText(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>(), TextFormattingOptions.InLineCode);
            message.AppendText(".");
            room.PostMessageOrThrow(message.Message);
        }

        private void HandleAuditPassed(UserWatcher watcher, ReviewItem audit)
        {
            var message = new MessageBuilder();
            var tag = audit.Tags[0];
            dbAccessor.InsertCompletedAuditEntry(watcher.UserID, tag);

            message.AppendPing(room.GetUser(watcher.UserID));
            message.AppendText("passed a");
            // Basic grammar correction. Not foolproof, but it'll do.
            message.AppendText("aeiou".Contains(char.ToLowerInvariant(tag[0])) ? "n " : " ");
            message.AppendText(tag, TextFormattingOptions.Tag);
            message.AppendText(" audit!");

            room.PostMessageOrThrow(message.Message);
        }

        private void HandleAuditFailed(UserWatcher watcher, ReviewItem audit)
        {
            // Do something...
        }

        private void HandleCurrentTagsChanged(UserWatcher watcher, List<string> oldTags)
        {
            var message = new MessageBuilder();

            message.AppendPing(room.GetUser(watcher.UserID));
            message.AppendText("It looks like you've finished reviewing ");

            if (oldTags.Count > 0)
            {
                message.AppendText(oldTags[0], TextFormattingOptions.Tag);

                if (oldTags.Count == 2)
                {
                    message.AppendText(" & ");
                    message.AppendText(oldTags[1], TextFormattingOptions.Tag);
                }
                else if (oldTags.Count == 3)
                {
                    message.AppendText(", ");
                    message.AppendText(oldTags[1], TextFormattingOptions.Tag);
                    message.AppendText(" & ");
                    message.AppendText(oldTags[2], TextFormattingOptions.Tag);
                }

                message.AppendText(". Is that right?");
            }
            else
            {
                return;
            }

            var m = room.PostMessage(message.Message);
            if (m == null) { throw new Exception("Unable to post message."); }
            tagReviewedConfirmationQueue[m] = oldTags;
        }

        private void HandleCurrentTagsChangedConfirmation(Message msg)
        {
            var parentMsgKv = tagReviewedConfirmationQueue.FirstOrDefault(kv => kv.Key.ID == msg.ParentID);
            if (parentMsgKv.Key == null) { return; }

            // Stop other people confirming someone else's message.
            var originalTarget = room[parentMsgKv.Key.ParentID].Author.ID;
            if (originalTarget != msg.Author.ID) { return; }

            if (yesRegex.IsMatch(parentMsgKv.Key.Content))
            {
                foreach (var tag in parentMsgKv.Value)
                {
                    dbAccessor.InsertNoItemsInFilterRecord(msg.Author.ID, tag);
                }
                var outMsg = "Ok, I've marked " + (parentMsgKv.Value.Count > 1 ? "them as completed tags." : "it as a completed tag.");
                room.PostReplyOrThrow(msg, outMsg);
            }
            else if (noRegex.IsMatch(parentMsgKv.Key.Content))
            {
                var outMsg = "Ok. Well don't forget to let me know when you've completed " + (parentMsgKv.Value.Count > 1 ? "them!" : "it!");
                room.PostReplyOrThrow(msg, outMsg);
            }
            else
            {
                return;
            }

            List<string> temp;
            tagReviewedConfirmationQueue.TryRemove(parentMsgKv.Key, out temp);
        }

        private void HandleException(UserWatcher watcher, Exception ex)
        {
            room.PostMessageOrThrow("error happened!\n" + ex.FullErrorMessage(Environment.NewLine));
        }
    }
}
