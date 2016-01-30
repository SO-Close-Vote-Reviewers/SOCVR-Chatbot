using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using TCL.Extensions;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class ApproveRequest : UserCommand
    {
        public override string ActionDescription => "Approves a pending permission request.";

        public override string ActionName => "Approve Request";

        public override string ActionUsage => "approve request [#]";

#warning this command needs a non-public permission group. "null" doesn't really work, but it can slide for now.
        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^approve request (\d+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var requestId = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Value
                    .Parse<int>();

                //look up the request
                var request = db.PermissionRequests.SingleOrDefault(x => x.Id == requestId);

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

                //all is good, set the new values
                request.ReviewingUserId = incomingChatMessage.Author.ID;
                request.Accepted = true;

                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"@{incomingChatMessage.Author.Name} has been added to the {request.RequestedPermissionGroup} group.");
            }
        }
    }
}
