using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class RejectRequest : RequestProcessingCommand
    {
        public override string ActionDescription => "Rejects a pending permission request.";

        public override string ActionName => "Reject Request";

        public override string ActionUsage => "reject request [#]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^reject request (\d{1,9})$";

        protected override string GetProcessSuccessfulMessage(PermissionRequest request, Room chatRoom)
        {
            return "Request processed successfully.";
        }

        protected override bool RequestValueAfterProcessing() => false;
    }
}
