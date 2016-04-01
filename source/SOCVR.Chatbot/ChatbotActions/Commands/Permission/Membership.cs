using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System.Linq;
using System.Text;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class Membership : UserCommand
    {
        public override string ActionDescription => "Shows a list of all permission groups, and the members in those permission groups.";

        public override string ActionName => "Membership";

        public override string ActionUsage => "membership";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^membership$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var permissionGroups = db.UserPermissions
                    .Include(x => x.User)
                    .GroupBy(x => x.PermissionGroup)
                    .OrderBy(x => x.Key)
                    .ToList();

                var outputMessageBuilder = new StringBuilder();

                foreach (var permissionGroup in permissionGroups)
                {
                    outputMessageBuilder.AppendLine("    " + permissionGroup.Key.ToString());

                    var usersInGroup = permissionGroup
                        .Select(x => x.User)
                        .Select(x => new
                        {
                            DisplayName = chatRoom.GetUser(x.ProfileId).Name,
                            ProfileId = x.ProfileId
                        })
                        .OrderBy(x => x.DisplayName);

                    foreach (var user in usersInGroup)
                    {
                        outputMessageBuilder.AppendLine($"        {user.DisplayName} ({user.ProfileId})");
                    }

                    outputMessageBuilder.AppendLine("    ");
                }

                chatRoom.PostReplyOrThrow(incomingChatMessage, "You want to know all the people who like to boss me around? I bet you're already on this list, and you just want to gloat.");
                chatRoom.PostMessageOrThrow(outputMessageBuilder.ToString());
            }
        }
    }
}
