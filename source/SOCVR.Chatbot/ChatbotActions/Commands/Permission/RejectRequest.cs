using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class RejectRequest : RequestProcessingCommand
    {
        public override string ActionDescription => "Rejects a pending permission request.";

        public override string ActionName => "Reject Request";

        public override string ActionUsage => "reject request [#]";

#warning this command needs a non-public permission group. "null" doesn't really work, but it can slide for now.
        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^reject request (\d+)$";

        protected override string GetProcessSuccessfulMessage(PermissionRequest request, Room chatRoom)
        {
            return "Request processed successfully.";
        }

        protected override bool RequestValueAfterProcessing() => false;
    }
}
