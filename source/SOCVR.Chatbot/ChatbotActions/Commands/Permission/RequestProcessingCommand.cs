using ChatExchangeDotNet;
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
    internal abstract class RequestProcessingCommand : UserCommand
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

                //look up the user who is processing this request.
                var processingUser = db.Users
                    .Include(x => x.Permissions)
                    .SingleOrDefault(x => x.ProfileId == incomingChatMessage.Author.ID);

                //if the user does not exist in the database, or the user does not belong to the group being requested
                if (processingUser == null || !request.RequestedPermissionGroup.In(processingUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"You need to be in the {request.RequestedPermissionGroup.ToString()} group in order to add people to it.");
                    return;
                }

                //check if there are any restrictions to joining the group
                switch (request.RequestedPermissionGroup)
                {
                    case PermissionGroup.Reviewer:
                        //user needs 3k rep
                        if (incomingChatMessage.Author.Reputation < 3000)
                        {
                            chatRoom.PostReplyOrThrow(incomingChatMessage, "Sorry, you need 3k reputation to join the Reviews group.");
                            return;
                        }
                        break;
                    case PermissionGroup.BotOwner:
                        //user needs to be in the Reviews group
                        if (!PermissionGroup.Reviewer.In(request.RequestingUser.Permissions.Select(x => x.PermissionGroup)))
                        {
                            chatRoom.PostReplyOrThrow(incomingChatMessage, "You need to be in the Reviews group before you can join the Bot Owners group.");
                            return;
                        }
                        break;
                }


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
                        UserId = request.RequestingUserId
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
