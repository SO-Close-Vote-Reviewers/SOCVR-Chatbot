using CVChatbot.Bot.Database;
using System.Collections.Generic;
using TCL.Extensions;
using System.Text.RegularExpressions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartingSession : UserCommand
    {
        private Regex ptn = new Regex(@"^(?:i'?m )?start(ing|ed)(?: now)?$", RegexObjOptions);

        public override string ActionDescription =>
            "Informs the chatbot that you are starting a new review session.";

        public override string ActionName => "Starting";

        public override string ActionUsage => "starting";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var chatUser = chatRoom.GetUser(incommingChatMessage.Author.ID);

            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            // First, check if the user has any open sessions, and close them
            var numberOfClosedSessions = da.EndAnyOpenSessions(incommingChatMessage.Author.ID);

            // Now record the new session
            da.StartReviewSession(incommingChatMessage.Author.ID);

            var replyMessages = new List<string>()
            {
                "Good luck!",
                "Happy reviewing!",
                "Don't get lost in the queue!",
                "Watch out for audits!",
                "May the Vote be with you!",
                "Make a dent!",
                "By the power of the Vote! Review!"
            };

            var outMessage = replyMessages.PickRandom();

            if (numberOfClosedSessions > 0) // If there was a closed session
            {
                // Append a message saying how many there were

                outMessage += " **Note:** You had {0} open {1}. I have closed {2}.".FormatInline(
                    numberOfClosedSessions,
                    numberOfClosedSessions > 1
                        ? "sessions"
                        : "session",
                    numberOfClosedSessions > 1
                        ? "them"
                        : "it");
            }

            chatRoom.PostReplyOrThrow(incommingChatMessage, outMessage);
        }
    }
}
