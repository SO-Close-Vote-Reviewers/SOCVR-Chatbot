using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using Microsoft.Data.Entity;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    class AddUserToGroup : PermissionUserCommand
    {
        public override string ActionDescription => "Manually adds a user to the given permission group.";

        public override string ActionName => "Add User To Group";

        public override string ActionUsage => "add [user id] to [group name]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^add (\d+) to ([\w ]+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var targetUserId = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Value
                .Parse<int>();

            //get the permission group from the chat message
            var rawRequestingPermissionGroup = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[2]
                .Value;

            //parse the string into the enum
            var requestingPermissionGroup = MatchInputToPermissionGroup(rawRequestingPermissionGroup);

            if (requestingPermissionGroup == null)
            {
                //we don't know what that permission group is
                chatRoom.PostReplyOrThrow(incomingChatMessage, "I don't know what that permission group is. Run `Membership` to see a list of permission groups.");
                return;
            }

            using (var db = new DatabaseContext())
            {
                //first, lookup the person running the command
                var processingUser = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == incomingChatMessage.Author.ID);

                //is the processing user in the correct group?
                if (!requestingPermissionGroup.Value.In(processingUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    //the person running the command in not in the group themselves, they can't add new members to it
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the {requestingPermissionGroup.ToString()} group in order to add people to it.");
                    return;
                }

                //lookup the target user
                var targetUser = db.Users
                    .Include(x => x.Permissions)
                    .Include(x => x.PermissionsRequested)
                    .SingleOrDefault(x => x.ProfileId == targetUserId);

                //if the user has never said a message in chat, the user will not exist in the database
                //add this user
                if (targetUser == null)
                {
                    targetUser = new Database.User()
                    {
                        ProfileId = targetUserId
                    };
                    db.Users.Add(targetUser);
                    db.SaveChanges();
                }

                //lookup the chat target user
                var chatTargetUser = chatRoom.GetUser(targetUserId);

                //does the target user already belong to the requested group?
                if (requestingPermissionGroup.Value.In(targetUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"{chatTargetUser.Name} is already in the {requestingPermissionGroup} group.");
                    return;
                }

                //check restrictions
                switch (requestingPermissionGroup.Value)
                {
                    case PermissionGroup.Reviewer:
                        //user needs 3k rep
                        if (chatTargetUser.Reputation < 3000)
                        {
                            chatRoom.PostReplyOrThrow(incomingChatMessage, "The target user need 3k reputation to join the Reviewers group.");
                            return;
                        }
                        break;
                    case PermissionGroup.BotOwner:
                        //user needs to be in the Reviews group
                        if (!PermissionGroup.Reviewer.In(targetUser.Permissions.Select(x => x.PermissionGroup)))
                        {
                            chatRoom.PostReplyOrThrow(incomingChatMessage, "The target user needs to be in the Reviewers group before they can join the Bot Owners group.");
                            return;
                        }
                        break;
                }

                //passed all checked, add the user to the group and approve any pending requests for that user/group
                targetUser.Permissions.Add(new UserPermission
                {
                    PermissionGroup = requestingPermissionGroup.Value,
                });

                var pendingRequestsForTargetUserAndGroup = targetUser
                    .PermissionsRequested
                    .Where(x => x.Accepted == null)
                    .Where(x => x.RequestedPermissionGroup == requestingPermissionGroup.Value);

                foreach (var pendingRequest in pendingRequestsForTargetUserAndGroup)
                {
                    //approve it
                    pendingRequest.Accepted = true;
                    pendingRequest.ReviewingUser = processingUser;
                }

                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"I've added @{chatTargetUser.GetChatFriendlyUsername()} to the {requestingPermissionGroup} group.");
            }
        }
    }
}
