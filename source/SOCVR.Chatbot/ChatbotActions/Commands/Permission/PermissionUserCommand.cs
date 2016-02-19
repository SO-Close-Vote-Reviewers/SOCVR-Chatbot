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

        protected PermissionGroupLeavabilityStatus CanTargetUserLeavePermissionGroup(PermissionGroup permissionGroup, int targetUserId)
        {
            using (var db = new DatabaseContext())
            {
                var targetUser = db.Users
                    .Include(x => x.Permissions)
                    .Where(x => x.ProfileId == targetUserId)
                    .Single();

                var userIsInGroup = targetUser.Permissions
                    .Where(x => x.PermissionGroup == permissionGroup)
                    .Any();

                if (!userIsInGroup)
                {
                    return PermissionGroupLeavabilityStatus.WasNotInGroup;
                }

                //passed all checks, user can leave group
                return PermissionGroupLeavabilityStatus.CanLeaveGroup;
            }
        }

        /// <summary>
        /// Tells if a user is allowed to add or remove other users from the given permission group.
        /// </summary>
        /// <param name="permissionGroup"></param>
        /// <param name="processingUserId"></param>
        /// <returns></returns>
        protected PermissionGroupModifiableStatus CanUserModifyMembershipForGroup(PermissionGroup permissionGroup, int processingUserId)
        {
            using (var db = new DatabaseContext())
            {
                var processingUser = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == processingUserId);

                //the only general restriction is that you are in the group you are trying to modify
                if (!permissionGroup.In(processingUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    return PermissionGroupModifiableStatus.NotInGroup;
                }

                //now check rules specific to each group
                switch (permissionGroup)
                {
                    case PermissionGroup.Reviewer:
                        return CanUserModifyReviewerGroup(permissionGroup, processingUser);
                    case PermissionGroup.BotOwner:
                        return CanUserModifyBotOwnersGroup(permissionGroup, processingUser);
                    default:
                        throw new Exception("Unknow permission group, unable to determine specific modification restrictions.");
                }
            }
        }

        private PermissionGroupModifiableStatus CanUserModifyReviewerGroup(PermissionGroup permissionGroup, User processingUser)
        {
            //user must be in the group for at least X days
            var joinedGroupAt = processingUser.Permissions
                .Single(x => x.PermissionGroup == permissionGroup)
                .JoinedOn;

            var minDaysNeededInGroup = ConfigurationAccessor.DaysInReviewersGroupBeforeProcessingRequests;
            var deltaDays = (DateTimeOffset.UtcNow - joinedGroupAt).TotalDays;

            if (deltaDays < minDaysNeededInGroup)
            {
                return PermissionGroupModifiableStatus.Reviewer_NotInGroupLongEnough;
            }

            //clear to modify group
            return PermissionGroupModifiableStatus.CanModifyGroupMembership;
        }

        private PermissionGroupModifiableStatus CanUserModifyBotOwnersGroup(PermissionGroup permissionGroup, User processingUser)
        {
            //there are no specific requirements for this group
            return PermissionGroupModifiableStatus.CanModifyGroupMembership;
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
            /// <summary>
            /// User is free to join the group
            /// </summary>
            CanJoinGroup,

            /// <summary>
            /// The user is already group
            /// </summary>
            AlreadyInGroup,

            /// <summary>
            /// The user does not have enough rep to join the Reviewer group
            /// </summary>
            Reviewer_NotEnoughRep,

            /// <summary>
            /// The user can't join the Bot Owner group because they are not in the Reviewer group
            /// </summary>
            BotOwner_NotInReviewerGroup,
        }

        protected enum PermissionGroupLeavabilityStatus
        {
            /// <summary>
            /// The user is free to leave the group
            /// </summary>
            CanLeaveGroup,

            /// <summary>
            /// The user is not in the group
            /// </summary>
            WasNotInGroup,
        }

        protected enum PermissionGroupModifiableStatus
        {
            /// <summary>
            /// User is free to modify the membership of the group
            /// </summary>
            CanModifyGroupMembership,

            /// <summary>
            /// User cannot modify membership of group because they are not in the group,
            /// </summary>
            NotInGroup,

            Reviewer_NotInGroupLongEnough,
        }
    }
}
