using SOCVR.Chatbot.Database;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class ApproveRequest : RequestProcessingCommand
    {
        public override string ActionDescription => "Approves a pending permission request.";

        public override string ActionName => "Approve Request";

        public override string ActionUsage => "approve request [#]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^approve request (\d{1,9})$";

        protected override string GetProcessSuccessfulMessage(PermissionRequest request, Room chatRoom)
        {
            var msg = new MessageBuilder();
            msg.AppendPing(chatRoom.GetUser(request.RequestingUserId));
            msg.AppendText($"has been added to the {request.RequestedPermissionGroup} group.");
            return msg.ToString();
        }

        protected override bool RequestValueAfterProcessing() => true;
    }
}
