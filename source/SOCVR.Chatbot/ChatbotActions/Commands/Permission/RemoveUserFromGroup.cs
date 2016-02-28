using SOCVR.Chatbot.Database;
using System.Linq;
using ChatExchangeDotNet;
using TCL.Extensions;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class RemoveUserFromGroup : PermissionUserCommand
    {
        public override string ActionDescription => "Manually removes a user from the given permission group.";

        public override string ActionName => "Remove User From Group";

        public override string ActionUsage => "remove [user id] from [group name]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^remove (\d+) from ([\w ]+)$";

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

                //is the processing user in the correct group?
                if (!indicatedPermissionGroup.Value.In(processingUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    //the person running the command in not in the group themselves, they can't add new members to it
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the {indicatedPermissionGroup.ToString()} group in order to remove people from it.");
                    return;
                }

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

                //if the target user is not in the indicated group
                if (!indicatedPermissionGroup.Value.In(targetUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"{chatTargetUser.Name} is not in the {indicatedPermissionGroup} group.");
                    return;
                }

                //don't need to check restrictions or requested permissions

                //remove the target user from the group
                var permissionToRemove = targetUser.Permissions.Single(x => x.PermissionGroup == indicatedPermissionGroup);
                targetUser.Permissions.Remove(permissionToRemove);

                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"I've removed {chatTargetUser.Name} from the {indicatedPermissionGroup} group.");
            }
        }
    }
}
