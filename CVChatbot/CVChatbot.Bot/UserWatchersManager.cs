/*
 * CVChatbot. Chatbot for the SO Close Vote Reviewers Chat Room.
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
    public class UserWatchersManager : IDisposable
    {
        private readonly ConcurrentDictionary<User, List<string>> tagReviewedConfirmationQueue;
        private readonly ConcurrentDictionary<int, DateTime> latestReviews;
        private readonly ConcurrentDictionary<int, DateTime> firstReviews;
        private readonly DatabaseAccessor dbAccessor;
        private readonly InstallationSettings initSettings;
        private readonly Room room;
        private readonly Regex yesRegex;
        private readonly Regex noRegex;
        private bool dispose;

        public UsersWatcher Watcher { get; private set; }



        public UserWatchersManager(ref Room chatRoom, InstallationSettings settings)
        {
            if (chatRoom == null) { throw new ArgumentNullException("chatRoom"); }
            if (settings == null) { throw new ArgumentNullException("settings"); }

            room = chatRoom;
            initSettings = settings;
            firstReviews = new ConcurrentDictionary<int, DateTime>();
            latestReviews = new ConcurrentDictionary<int, DateTime>();
            tagReviewedConfirmationQueue = new ConcurrentDictionary<User, List<string>>();
            yesRegex = new Regex(@"(?i)\by[ue][aps]h?\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            noRegex = new Regex(@"(?i)\bno*(pe|t)?\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            dbAccessor = new DatabaseAccessor(settings.DatabaseConnectionString);

            InitialiseWatcher();

            // Look out for registered members that haven't joined in a while.
            chatRoom.EventManager.ConnectListener(EventType.UserEntered, new Action<User>(u => AddNewUser(u.ID)));

            // Listen out for tracking new users.
            chatRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleTrackUserCommand(m)));

            // Listen for tag confirmations.
            chatRoom.EventManager.ConnectListener(EventType.MessageReply, new Action<Message, Message>((parent, reply) => HandleCurrentTagsChangedConfirmation(reply)));
        }

        ~UserWatchersManager()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) { return; }
            dispose = true;

            Watcher.Dispose();

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



        private void InitialiseWatcher()
        {
            var pingable = room.GetPingableUsers();
            var users = new HashSet<int>();

            foreach (var user in pingable)
            {
                if (dbAccessor.GetRegisteredUserByChatProfileId(user.ID) == null ||
                    user.Reputation < 3000)
                { continue; }

                users.Add(user.ID);
            }

            Watcher = new UsersWatcher(users);

            //TODO: You may want to change this.
            //Watcher.ReviewThroughput = ?

            foreach (var id in users)
            {
                HookUpUserEvents(id);
            }
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
                Watcher.Users.ContainsKey(userID))
            {
                return;
            }

            Watcher.AddUser(userID);
            HookUpUserEvents(userID);
        }

        private void HookUpUserEvents(int id)
        {
            Watcher.Users[id].EventManager.ConnectListener(UserEventType.ItemReviewed,
                    new Action<ReviewItem>(r => HandleItemReviewed(Watcher.Users[id], r)));

            Watcher.Users[id].EventManager.ConnectListener(UserEventType.ReviewLimitReached,
                new Action(() => HandleReviewLimitReached(Watcher.Users[id])));

            Watcher.Users[id].EventManager.ConnectListener(UserEventType.AuditPassed,
                new Action<ReviewItem>(r => HandleAuditPassed(Watcher.Users[id], r)));

            Watcher.Users[id].EventManager.ConnectListener(UserEventType.AuditFailed,
                new Action<ReviewItem>(r => HandleAuditFailed(Watcher.Users[id], r)));

            Watcher.Users[id].EventManager.ConnectListener(UserEventType.CurrentTagsChanged,
                new Action<List<string>>((oldTags) => HandleCurrentTagsChanged(Watcher.Users[id], oldTags)));

            Watcher.Users[id].EventManager.ConnectListener(UserEventType.InternalException,
                new Action<Exception>(ex => HandleException(ex)));
        }

        private void HandleItemReviewed(SOCVRDotNet.User user, ReviewItem review)
        {
            var reviewTime = review.Results.First(rr => rr.UserID == user.ID).Timestamp;

            if (!firstReviews.ContainsKey(user.ID) || firstReviews[user.ID].Day != DateTime.UtcNow.Day)
            {
                firstReviews[user.ID] = reviewTime;
                latestReviews[user.ID] = reviewTime;
                var msg = new MessageBuilder();
                msg.AppendPing(room.GetUser(user.ID));
                msg.AppendText("I've noticed you've started reviewing! I'll update your session record.");
                room.PostMessageFast(msg);
            }

            latestReviews[user.ID] = reviewTime;


            //TODO: I have no idea what we're doing with the below (old code).
            //var chatUser = room.GetUser(user.ID);
            //var numberOfClosedSessions = dbAccessor.EndAnyOpenSessions(user.ID);

            //dbAccessor.StartReviewSession(user.ID);

            //var message = new MessageBuilder();

            //message.AppendPing(chatUser);
            //message.AppendText("I've noticed you've started reviewing. I'll make a new session for you. Good luck! ");

            //if (numberOfClosedSessions > 0)
            //{
            //    // Append a message saying how many there were.
            //    message.AppendText("Note:", TextFormattingOptions.Bold);
            //    message.AppendText(" You had {0} open {1}. I have closed {2}.".FormatInline(
            //        numberOfClosedSessions,
            //        numberOfClosedSessions > 1
            //            ? "sessions"
            //            : "session",
            //        numberOfClosedSessions > 1
            //            ? "them"
            //            : "it"));
            //}

            //room.PostMessageOrThrow(message.Message);
        }

        private void HandleReviewLimitReached(SOCVRDotNet.User user)
        {
            var msgText = "You've completed {0} CV review items today, thanks! " + 
                           "The time between your first and last review today was {1} minutes, " + 
                           "averaging to a review every {2} seconds.";

            var revDur = latestReviews[user.ID] - firstReviews[user.ID];
            var avg = revDur.TotalSeconds / user.ReviewStatus.ReviewsCompletedCount;

            msgText = msgText.FormatInline(
                user.ReviewStatus.ReviewsCompletedCount,
                Math.Round(revDur.TotalMinutes),
                Math.Round(avg, 1));

            var msg = new MessageBuilder();
            msg.AppendPing(room.GetUser(user.ID));
            msg.AppendText(msgText);
            room.PostMessageFast(msg);
        }

        private void HandleAuditPassed(SOCVRDotNet.User user, ReviewItem audit)
        {
            var message = new MessageBuilder();
            var tag = audit.Tags[0];
            dbAccessor.InsertCompletedAuditEntry(user.ID, tag);

            message.AppendText(room.GetUser(user.ID).GetChatFriendlyUsername());
            message.AppendText(" passed a");
            // Basic grammar correction. Not foolproof, but it'll do.
            message.AppendText("aeiou".Contains(char.ToLowerInvariant(tag[0])) ? "n " : " ");
            message.AppendText(tag, TextFormattingOptions.Tag, WhiteSpace.Space);
            message.AppendLink("audit!", "http://stackoverflow.com/review/close/" + audit.ID);

            room.PostMessageOrThrow(message.Message);
        }

        private void HandleAuditFailed(SOCVRDotNet.User user, ReviewItem audit)
        {
            // Do something...
        }

        private void HandleCurrentTagsChanged(SOCVRDotNet.User user, List<string> oldTags)
        {
            var message = new MessageBuilder();
            var chatUser = room.GetUser(user.ID);

            message.AppendPing(chatUser);
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

            room.PostMessageOrThrow(message.Message);

            tagReviewedConfirmationQueue[chatUser] = oldTags;
        }

        private void HandleCurrentTagsChangedConfirmation(Message reply)
        {
            var tagConfirmationKv = tagReviewedConfirmationQueue.FirstOrDefault(kv => kv.Key.ID == reply.Author.ID);
            if (tagConfirmationKv.Key == null) { return; }

            if (yesRegex.IsMatch(reply.Content))
            {
                foreach (var tag in tagConfirmationKv.Value)
                {
                    dbAccessor.InsertNoItemsInFilterRecord(reply.Author.ID, tag);
                }
                var outMsg = "Ok, I've marked " + (tagConfirmationKv.Value.Count > 1 ?
                    "them as completed tags." :
                    "it as a completed tag.");
                room.PostReplyOrThrow(reply, outMsg);
            }
            else if (noRegex.IsMatch(reply.Content))
            {
                var outMsg = "Sorry, my mistake.";
                room.PostReplyOrThrow(reply, outMsg);
            }
            else
            {
                return;
            }

            List<string> temp;
            tagReviewedConfirmationQueue.TryRemove(tagConfirmationKv.Key, out temp);
        }

        private void HandleException(Exception ex)
        {
            var headerLine = "An error happened in User Watcher Manager";
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
