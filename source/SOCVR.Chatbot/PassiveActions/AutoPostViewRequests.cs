using ChatExchangeDotNet;
using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using TCL.Extensions;

namespace SOCVR.Chatbot.PassiveActions
{
    internal class AutoPostViewRequests
    {
        private static DateTimeOffset LastActionRun = DateTimeOffset.MinValue;
        private static List<DateTimeOffset> BotOwnerMessagePostTimes = new List<DateTimeOffset>();

        public bool ShouldActionBeRan(Message chatMessage)
        {
            using (var db = new DatabaseContext())
            {
                //if this message was posted by a Bot Owner add the current time to the list
                var dbUser = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == chatMessage.Author.ID);

                if (PermissionGroup.BotOwner.In(dbUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    BotOwnerMessagePostTimes.Add(DateTimeOffset.UtcNow);
                }

                //clear any times that have expired
                var expiredTimes = BotOwnerMessagePostTimes.Where(x => (DateTimeOffset.UtcNow - x).TotalMinutes > 5).ToList();
                foreach (var expiredTime in expiredTimes)
                {
                    BotOwnerMessagePostTimes.Remove(expiredTime);
                }

                //are there any active permission requests?
                var anyActivePermissionRequests = db.PermissionRequests
                    .Where(x => x.Accepted == null)
                    .Any();

                //if no, don't run
                if (!anyActivePermissionRequests)
                    return false;

                //it's been >6 hours since the last time this message was posted?
                var deltaHours = (DateTimeOffset.UtcNow - LastActionRun).TotalHours;

                if (deltaHours < 6)
                    return false;

                //is there at least 3 messages by BO's that have not expired?
                if (BotOwnerMessagePostTimes.Count < 3)
                    return false;

                //passed all tests
                return true;
            }
        }

        public void RunAction(Message chatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var pendingRequests = db.PermissionRequests
                    .Include(x => x.RequestingUser)
                    .Where(x => x.Accepted == null)
                    .OrderBy(x => x.RequestedOn)
                    .ToList();

                if (!pendingRequests.Any())
                {
                    //this should not happen
                    throw new Exception("No active permission requests but tried to auto-post.");
                }

                var tableText = pendingRequests.ToStringTable(
                    new[]
                    {
                        "Request #",
                        "Display Name",
                        "User Id",
                        "Requesting",
                        "Requested At"
                    },
                    x => x.Id,
                    x => chatRoom.GetUser(x.RequestingUser.ProfileId),
                    x => x.RequestingUser.ProfileId,
                    x => x.RequestedPermissionGroup.ToString(),
                    x => x.RequestedOn.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));

                chatRoom.PostMessageOrThrow($"The following is a list of users requesting access to a permission group.");
                chatRoom.PostMessageOrThrow(tableText);
            }

            //update the last post time
            LastActionRun = DateTimeOffset.UtcNow;
        }
    }
}
