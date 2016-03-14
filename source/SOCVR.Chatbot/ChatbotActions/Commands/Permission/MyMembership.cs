using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class MyMembership : UserCommand
    {
        public override string ActionDescription => "Shows the permission groups you are a part of.";

        public override string ActionName => "My Membership";

        public override string ActionUsage => "my membership";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^my memberships?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var userData = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == incomingChatMessage.Author.ID);

                var permisionGroupsUserIsIn = userData.Permissions
                    .Select(x => x.PermissionGroup)
                    .ToList();

                if (!permisionGroupsUserIsIn.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "You are not in any permission groups.");
                    return;
                }

                var groupsString = permisionGroupsUserIsIn
                    .Select(x => x.ToString())
                    .CreateOxforCommaListString();

                var messageText = $"You are in the {groupsString} {(permisionGroupsUserIsIn.Count == 1 ? "group" : "groups")}.";
                chatRoom.PostReplyOrThrow(incomingChatMessage, messageText);
            }
        }
    }
}
