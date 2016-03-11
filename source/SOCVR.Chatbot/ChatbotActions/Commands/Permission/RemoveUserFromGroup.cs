using SOCVR.Chatbot.Database;
using System.Linq;
using ChatExchangeDotNet;
using TCL.Extensions;
using Microsoft.Data.Entity;
using SOCVR.Chatbot.Configuration;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class RemoveUserFromGroup : PermissionUserCommand
    {
        public override string ActionDescription => "Manually removes a user from the given permission group.";

        public override string ActionName => "Remove User From Group";

        public override string ActionUsage => "remove [user id] from [group name]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^remove (\d{1,9}) from ([\w ]+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var targetUserId = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Value
                .Parse<int>();

            if (!ChatExchangeDotNet.User.Exists(chatRoom.Meta, targetUserId))
            {
                chatRoom.PostReplyOrThrow(incomingChatMessage, "Sorry, I couldn't find a user with that ID.");
                return;
            }

            //get the permission group from the chat message
            var rawRequestingPermissionGroup = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[2]
                .Value;

            //parse the string into the enum
            var indicatedPermissionGroup = MatchInputToPermissionGroup(rawRequestingPermissionGroup);

            if (indicatedPermissionGroup == null)
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

                //lookup the target user
                var targetUser = db.Users
                    .Include(x => x.Permissions)
                    .Include(x => x.PermissionsRequested)
                    .SingleOrDefault(x => x.ProfileId == targetUserId);

                //if the user has never said a message in chat, the user will not exist in the database
                //add this user (just so we have a reference to the user later)
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

                //check restrictions on processing user
                var processingUserAbilityStatus = CanUserModifyMembershipForGroup(indicatedPermissionGroup.Value, processingUser.ProfileId);

                if (processingUserAbilityStatus != PermissionGroupModifiableStatus.CanModifyGroupMembership)
                {
                    switch (processingUserAbilityStatus)
                    {
                        case PermissionGroupModifiableStatus.NotInGroup:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the {indicatedPermissionGroup.Value} group in order to add people to it.");
                            break;
                        case PermissionGroupModifiableStatus.Reviewer_NotInGroupLongEnough:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the Reviewer group for at least {ConfigurationAccessor.DaysInReviewersGroupBeforeProcessingRequests} days before you can process requests.");
                            break;
                    }

                    return;
                }
                //else, there was no problems with using this processing user

                //check restrictions on target user
                var canJoinStatus = CanTargetUserLeavePermissionGroup(indicatedPermissionGroup.Value, targetUserId);

                if (canJoinStatus != PermissionGroupLeavabilityStatus.CanLeaveGroup)
                {
                    switch (canJoinStatus)
                    {
                        case PermissionGroupLeavabilityStatus.WasNotInGroup:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"{chatTargetUser.Name} is not in the {indicatedPermissionGroup.Value} group.");
                            break;
                    }

                    return;
                }
                //else, user can leave group, continue on

                //remove the target user from the group
                var permissionToRemove = targetUser.Permissions.Single(x => x.PermissionGroup == indicatedPermissionGroup);
                targetUser.Permissions.Remove(permissionToRemove);

                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"I've removed {chatTargetUser.Name} from the {indicatedPermissionGroup} group.");
            }
        }
    }
}
