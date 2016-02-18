using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal abstract class PermissionUserCommand : UserCommand
    {
        protected PermissionGroup? MatchInputToPermissionGroup(string userInput)
        {
            //take the input, remove spaces, lower case it
            userInput = userInput
                .Replace(" ", "")
                .ToLowerInvariant();

            if (userInput.EndsWith("s"))
            {
                userInput = userInput.Remove(userInput.Length - 2);
            }

            var allPermissionGroups = Enum.GetValues(typeof(PermissionGroup))
                .OfType<PermissionGroup>()
                .Select(x => new
                {
                    EnumVal = x,
                    MatchVal = x.ToString().ToLowerInvariant()
                });

            var matchingPermissionGroup = allPermissionGroups
                .SingleOrDefault(x => x.MatchVal == userInput);

            return matchingPermissionGroup?.EnumVal;
        }

        protected PermissionGroupJoinabilityStatus CanTargetUserJoinPermissionGroup(PermissionGroup permissionGroup, int targetUserId)
        {
            using (var db = new DatabaseContext())
            {
                //lookup the target user
                var targetUser = db.Users
                    .Include(x => x.Permissions)
                    .Include(x => x.PermissionsRequested)
                    .SingleOrDefault(x => x.ProfileId == targetUserId);

                //if the user has never said a message in chat, the user will not exist in the database
                //add this user
                if (targetUser == null)
                {
                    targetUser = new User()
                    {
                        ProfileId = targetUserId
                    };
                    db.Users.Add(targetUser);
                    db.SaveChanges();
                }

                //does the target user already belong to the requested group?
                if (permissionGroup.In(targetUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    return PermissionGroupJoinabilityStatus.AlreadyInGroup;
                }

                switch (permissionGroup)
                {
                    case PermissionGroup.Reviewer:

                        break;
                    case PermissionGroup.BotOwner:

                        break;
                }
            }
        }

        private PermissionGroupJoinabilityStatus CanUserJoinReviewersGroup(int targetUserId)
        {

        }

        protected enum PermissionGroupJoinabilityStatus
        {
            CanJoinGroup,
            AlreadyInGroup,

        }
    }
}
