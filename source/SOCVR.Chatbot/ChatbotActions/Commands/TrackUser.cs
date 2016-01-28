using System.Text.RegularExpressions;
using SOCVR.Chatbot.Bot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class TrackUser : UserCommand
    {
        private Regex ptn = new Regex(@"^(?:add|track) user (\d+)$", RegexObjOptions);

        public override string ActionDescription => "Adds the user to the registered users list.";

        public override string ActionName => "Add user";

        public override string ActionUsage => "(add | track) user <chat id>";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Owner;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var userIdToAdd = RegexMatchingObject
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            DatabaseAccessor da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var existingUser = da.GetRegisteredUserByChatProfileId(userIdToAdd);

            if (existingUser != null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "That user is already in the system!");
                return;
            }

            da.AddUserToRegisteredUsersList(userIdToAdd);

            var chatUser = chatRoom.GetUser(userIdToAdd);
            chatRoom.PostReplyOrThrow(incommingChatMessage, $"Ok, I added {chatUser.Name} ({chatUser.ID}) to the ---stalked--- tracked users list.");
        }
    }
}
