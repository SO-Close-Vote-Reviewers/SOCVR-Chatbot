﻿using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System;
using System.Linq;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions.Annoucements
{
    /// <summary>
    /// Posts a general message detailing the requests to join various permission groups
    /// </summary>
    internal class PostPendingPermissoinRequests : Announcement
    {
        public override void RunAction(Room chatRoom)
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

                chatRoom.PostMessageOrThrow("The following is a list of users requesting access to a permission group.");
                chatRoom.PostMessageOrThrow(tableText);
            }
        }
    }
}
