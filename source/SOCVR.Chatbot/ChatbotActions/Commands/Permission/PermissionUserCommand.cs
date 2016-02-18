using Microsoft.Data.Entity;
using SOCVR.Chatbot.Configuration;
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

        protected PermissionGroupJoinabilityStatus CanTargetUserJoinPermissionGroup(PermissionGroup permissionGroup, int targetUserId, ChatExchangeDotNet.Room chatRoom)
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

                //now that we have checked the general rules, switch on the specific group
                //and check any specific restrictions
                switch (permissionGroup)
                {
                    case PermissionGroup.Reviewer:
                        return CanUserJoinReviewersGroup(targetUser, chatRoom);
                    case PermissionGroup.BotOwner:
                        return CanUserJoinBotOwnersGroup(targetUser);
                    default:
                        throw new Exception("Unknow permission group, unable to determine specific joining restrictions.");
                }
            }
        }

        protected object CanTargetUserLeavePermissionGroup(PermissionGroup permissionGroup, int targetUserId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tells if a user is allowed to add or remove other users from the given permission group.
        /// </summary>
        /// <param name="permissionGroup"></param>
        /// <param name="processingUserId"></param>
        /// <returns></returns>
        protected object CanUserModifyMembershipForGroup(PermissionGroup permissionGroup, int processingUserId)
        {
            throw new NotImplementedException();
        }

        private PermissionGroupJoinabilityStatus CanUserJoinReviewersGroup(User targetUser, ChatExchangeDotNet.Room chatRoom)
        {
            //check user rep
            var repRequirement = ConfigurationAccessor.RepRequirementToJoinReviewers;
            var targetUserRep = chatRoom.GetUser(targetUser.ProfileId).Reputation;

            if (targetUserRep < repRequirement)
            {
                return PermissionGroupJoinabilityStatus.Reviewer_NotEnoughRep;
            }

            return PermissionGroupJoinabilityStatus.CanJoinGroup;
        }

        private PermissionGroupJoinabilityStatus CanUserJoinBotOwnersGroup(User targetUser)
        {
            //if the target user is not in the Reviewer group, reject
            if (!PermissionGroup.Reviewer.In(targetUser.Permissions.Select(x => x.PermissionGroup)))
            {
                return PermissionGroupJoinabilityStatus.BotOwner_NotInReviewerGroup;
            }

            return PermissionGroupJoinabilityStatus.CanJoinGroup;
        }

        protected enum PermissionGroupJoinabilityStatus
        {
            CanJoinGroup,
            AlreadyInGroup,

            Reviewer_NotEnoughRep,

            BotOwner_NotInReviewerGroup,
        }
    }
}
