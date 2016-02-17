using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    class RequestPermissionToGroup : PermissionUserCommand
    {
        public override string ActionDescription => "Submits a request for the user to be added to a given permission group.";

        public override string ActionName => "Request permission to group";

        public override string ActionUsage => "request permission to [group name]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^request permission to (\w+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            //get the permission group from the chat message
            var rawRequestingPermissionGroup = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
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
                //look up user
                var requestingUser = db.Users
                    .Include(x => x.Permissions)
                    .Include(x => x.PermissionsRequested)
                    .SingleOrDefault(x => x.ProfileId == incomingChatMessage.Author.ID);

                //is the user already in the group?
                if (requestingPermissionGroup.Value.In(requestingUser.Permissions.Select(x => x.PermissionGroup)))
                {
                    //the user is already in that group
                    chatRoom.PostReplyOrThrow(incomingChatMessage, $"You are already in the {rawRequestingPermissionGroup} group.");
                    return;
                }

                //lookup the latest request for this user/group
                var latestPermissionRequestForThisGroup = requestingUser
                    .PermissionsRequested
                    .Where(x => x.RequestedPermissionGroup == requestingPermissionGroup.Value)
                    .OrderByDescending(x => x.RequestedOn)
                    .FirstOrDefault();

                if (latestPermissionRequestForThisGroup == null) //check if such a request has been made before
                {
                    //user has not made a request for this group before
                    //move on
                }
                else if (latestPermissionRequestForThisGroup.Accepted == null) //check if this latest request is still pending
                {
                    //user needs to wait
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "There is already a request to get you this permission, please be patent.");
                    return;
                }
                else if (latestPermissionRequestForThisGroup.Accepted == false) //check if this latest requst was rejected
                {
                    //was the request made within the last 48 hours?
                    var deltaTime = DateTimeOffset.UtcNow - latestPermissionRequestForThisGroup.RequestedOn;

                    if (deltaTime.TotalHours < 48)
                    {
                        //yes, the user needs to wait until the cool down expires to ask again
                        chatRoom.PostReplyOrThrow(incomingChatMessage, $"Sorry, your latest request for this permission was denied. Please wait ${deltaTime.ToUserFriendlyString()} to request again.");
                        return;
                    }
                }

                //check if there are any restrictions to joining the group
                switch (requestingPermissionGroup.Value)
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
                        if (!PermissionGroup.Reviewer.In(requestingUser.Permissions.Select(x => x.PermissionGroup)))
                        {
                            chatRoom.PostReplyOrThrow(incomingChatMessage, "You need to be in the Reviews group before you can join the Bot Owners group.");
                            return;
                        }
                        break;
                }

                //at this point we are good to make the request
                var newRequest = new PermissionRequest()
                {
                    Accepted = null,
                    RequestedOn = DateTimeOffset.UtcNow,
                    RequestedPermissionGroup = requestingPermissionGroup.Value,
                    RequestingUser = requestingUser,
                    RequestingUserId = requestingUser.ProfileId
                };

                db.PermissionRequests.Add(newRequest);
                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"I've created request #{newRequest.Id} to get you in the {requestingPermissionGroup.Value} group.");
            }
        }
    }
}
