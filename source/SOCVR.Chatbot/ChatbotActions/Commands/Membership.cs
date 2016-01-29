using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System.Linq;
using System.Text;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class Membership : UserCommand
    {
        public override string ActionDescription => "Shows a list of all permission groups, and the members in those permission groups.";

        public override string ActionName => "Membership";

        public override string ActionUsage => "membership";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^membership$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var usersInGroups = db.UserPermissions
                    .Include(x => x.User)
                    .GroupBy(x => x.PermissionGroup)
                    .OrderBy(x => x.Key)
                    .ToList();

                var outputMessageBuilder = new StringBuilder();

                foreach (var permissionGroup in usersInGroups)
                {
                    outputMessageBuilder.AppendLine("    " + permissionGroup.Key.ToString());

                    foreach (var user in permissionGroup.Select(x => x.User))
                    {
                        var displayName = chatRoom.GetUser(user.ProfileId).Name;
                        outputMessageBuilder.AppendLine($"        {displayName} ({user.ProfileId})");
                    }

                    outputMessageBuilder.AppendLine("    ");
                }

                chatRoom.PostReplyOrThrow(incomingChatMessage, "Current users in permission groups:");
                chatRoom.PostMessageOrThrow(outputMessageBuilder.ToString());
            }
        }
    }
}
