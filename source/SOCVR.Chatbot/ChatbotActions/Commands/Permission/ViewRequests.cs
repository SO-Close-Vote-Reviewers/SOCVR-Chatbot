using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class ViewRequests : UserCommand
    {
        public override string ActionDescription => "Shows all pending permission requests.";

        public override string ActionName => "View Requests";

        public override string ActionUsage => "view requests";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^view requests$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var pendingRequests = db.PermissionRequests
                    .Include(x => x.RequestingUser)
                    .Where(x => x.Accepted == null)
                    .OrderBy(x => x.RequestedOn)
                    .ToList();

                if (!pendingRequests.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "There are no users requesting access to a permission group.");
                    return;
                }

                var tableText = pendingRequests.ToStringTable(
                    new[]
                    {
                        "Request #",
                        "Display Name",
                        "User Id",
                        "Requesting",
                        "Requested At"
                    },
                    x => x.Id,
                    x => chatRoom.GetUser(x.RequestingUser.ProfileId),
                    x => x.RequestingUser.ProfileId,
                    x => x.RequestedPermissionGroup.ToString(),
                    x => x.RequestedOn.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));

                chatRoom.PostReplyOrThrow(incomingChatMessage, "The following is a list of users requesting access to a permission group. If you are part of these groups, you may use the commands `approve request [#]` or `reject request [#]` to process them.");
                chatRoom.PostMessageFast(tableText);
            }
        }
    }
}
