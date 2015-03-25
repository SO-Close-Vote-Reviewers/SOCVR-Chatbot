using ChatExchangeDotNet;
using CVChatbot.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartingSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var chatUser = chatRoom.GetUser(incommingChatMessage.AuthorID);

            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            // first, check if the user has any open sessions, and close them
            int numberOfClosedSessions = da.EndAnyOpenSessions(incommingChatMessage.AuthorID);

            // now record the new session
            da.StartReviewSession(incommingChatMessage.AuthorID);

            var replyMessages = new List<string>()
            {
                "Good luck!",
                "Happy reviewing!",
                "Don't get lost in the queue!",
                "Watch out for audits!",
                "May the Vote be with you!",
                "May Shog9's Will be done.",
                "By the power of the Vote! Review!"
            };

            var outMessage = replyMessages.PickRandom();

            if (numberOfClosedSessions > 0) //if there was a closed session
            {
                //append a message saying how many there were

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

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:i'?m )?start(ing|ed)(?: now)?$";
        }

        public override string GetActionName()
        {
            return "Starting";
        }

        public override string GetActionDescription()
        {
            return "Informs the chatbot that you are starting a new review session.";
        }

        public override string GetActionUsage()
        {
            return "starting";
        }
    }
}
