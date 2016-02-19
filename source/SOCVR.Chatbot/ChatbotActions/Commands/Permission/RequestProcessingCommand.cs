using ChatExchangeDotNet;
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
    internal abstract class RequestProcessingCommand : PermissionUserCommand
    {
        /// <summary>
        /// Returns either a "true" or "false" for if the child command
        /// is "Accept" or "Reject".
        /// </summary>
        /// <returns></returns>
        protected abstract bool RequestValueAfterProcessing();

        protected abstract string GetProcessSuccessfulMessage(PermissionRequest request, Room chatRoom);

        public override sealed void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var requestId = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Value
                    .Parse<int>();

                //look up the request
                var request = db.PermissionRequests
                    .Include(x => x.RequestingUser)
                    .Include(x => x.RequestingUser.Permissions)
                    .SingleOrDefault(x => x.Id == requestId);

                //does it exist?
                if (request == null)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I can't find that permission request. Run `view requests` to see the current list.");
                    return;
                }

                //has the request already been processed?
                if (request.Accepted != null)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "That request has already been handled.");
                    return;
                }

                var processingUserId = incomingChatMessage.Author.ID;

                //check restrictions on processing user
                var processingUserAbilityStatus = CanUserModifyMembershipForGroup(request.RequestedPermissionGroup, processingUserId);

                if (processingUserAbilityStatus != PermissionGroupModifiableStatus.CanModifyGroupMembership)
                {
                    switch (processingUserAbilityStatus)
                    {
                        case PermissionGroupModifiableStatus.NotInGroup:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the {request.RequestedPermissionGroup} group in order to add people to it.");
                            break;
                        case PermissionGroupModifiableStatus.Reviewer_NotInGroupLongEnough:
                            chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the Reviewer group for at least {ConfigurationAccessor.DaysInReviewersGroupBeforeProcessingRequests} days before you can process requests.");
                            break;
                    }

                    return;
                }
                //else, there was no problems with using this processing user


                //check restrictions on target user
                if (RequestValueAfterProcessing() == true) //approve, joining group
                {
                    //check if user can join group
                    var canJoinStatus = CanTargetUserJoinPermissionGroup(request.RequestedPermissionGroup, request.RequestingUser.ProfileId, chatRoom);

                    if (canJoinStatus != PermissionGroupJoinabilityStatus.CanJoinGroup)
                    {
                        switch (canJoinStatus)
                        {
                            case PermissionGroupJoinabilityStatus.AlreadyInGroup:
                                chatRoom.PostReplyOrThrow(incomingChatMessage, "The target user is already in the requested group. This is most likely a bug. cc @gunr2171.");
                                break;
                            case PermissionGroupJoinabilityStatus.BotOwner_NotInReviewerGroup:
                                chatRoom.PostReplyOrThrow(incomingChatMessage, "The target user needs to be in the Reviewer group before they can join the Bot Owners group.");
                                break;
                            case PermissionGroupJoinabilityStatus.Reviewer_NotEnoughRep:
                                chatRoom.PostReplyOrThrow(incomingChatMessage, $"The target user needs at least {ConfigurationAccessor.RepRequirementToJoinReviewers} rep to join the Reviewer group");
                                break;
                        }

                        return;
                    }
                    
                    //else, user can join group, continue on
                }
                //else, reject. Don't need to check anything for the target user becuase the target user doesn't get modified


                //all is good, set the new values
                request.ReviewingUserId = incomingChatMessage.Author.ID;
                request.Accepted = RequestValueAfterProcessing();

                //if the request is approved
                if (RequestValueAfterProcessing() == true)
                {
                    //add to permissions list of requesting user
                    var newUserPermission = new UserPermission()
                    {
                        PermissionGroup = request.RequestedPermissionGroup,
                        UserId = request.RequestingUserId,
                        JoinedOn = DateTimeOffset.UtcNow
                    };
                    db.UserPermissions.Add(newUserPermission);

                    //if the request was for the reviewer group
                    if (request.RequestedPermissionGroup == PermissionGroup.Reviewer)
                    {
                        //opt-in
                        request.RequestingUser.OptInToReviewTracking = true;
                        request.RequestingUser.LastTrackingPreferenceChange = DateTimeOffset.Now;
                    }
                }

                db.SaveChanges();

                var outputMessage = GetProcessSuccessfulMessage(request, chatRoom);
                chatRoom.PostReplyOrThrow(incomingChatMessage, outputMessage);
            }
        }
    }
}
