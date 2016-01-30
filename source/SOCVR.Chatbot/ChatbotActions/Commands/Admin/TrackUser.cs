using System.Collections.Generic;
using System.Linq;
using SOCVR.Chatbot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class TrackUser : UserCommand
    {
        public override string ActionDescription => "Adds the user to the registered users list.";

        public override string ActionName => "Add user";

        public override string ActionUsage => "add user <chat id>";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        protected override string RegexMatchingPattern => @"^add user (\d+)$";

#warning this command is going away / renamed

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var userIdToAdd = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Value
                .Parse<int>();

            using (var db = new DatabaseContext())
            {
                var userExists = db.Users.Any(x => x.ProfileId == userIdToAdd);

                if (userExists)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "That user is already in the system!");
                    return;
                }

                //TODO: need gunr's seal of approval on the DB code below.
                var user = new User()
                {
                    ProfileId = userIdToAdd,
                    Permissions = new List<UserPermission>()
                };

                user.Permissions.Add(new UserPermission
                {
                    PermissionGroup = PermissionGroup.Reviewer,
                    User = user,
                    UserId = userIdToAdd
                });

                db.Users.Add(user);

                var chatUser = chatRoom.GetUser(userIdToAdd);
                chatRoom.PostReplyOrThrow(incomingChatMessage, $"Ok, I added {chatUser.Name} ({chatUser.ID}) to the tracked users list.");
            }
        }
    }
}
