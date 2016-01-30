using System;
using SOCVR.Chatbot.Database;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class ApproveRequest : RequestProcessingCommand
    {
        public override string ActionDescription => "Approves a pending permission request.";

        public override string ActionName => "Approve Request";

        public override string ActionUsage => "approve request [#]";

#warning this command needs a non-public permission group. "null" doesn't really work, but it can slide for now.
        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^approve request (\d+)$";

        protected override string GetProcessSuccessfulMessage(PermissionRequest request, Room chatRoom)
        {
            var displayName = chatRoom.GetUser(request.RequestingUserId).Name;
            return $"@{displayName} has been added to the {request.RequestedPermissionGroup} group.";
        }

        protected override bool RequestValueAfterProcessing() => true;
    }
}
