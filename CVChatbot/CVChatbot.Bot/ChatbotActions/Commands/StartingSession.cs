using ChatExchangeDotNet;
using CVChatbot.Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartingSession : UserCommand
    {
        public override void RunAction(Message userMessage, Room chatRoom)
        {
            var chatUser = chatRoom.GetUser(userMessage.AuthorID);

            // Start a new review session.
            using (var db = new CVChatBotEntities())
            {
                var registedUser = db.RegisteredUsers
                    .Single(x => x.ChatProfileId == userMessage.AuthorID);

                var newSession = new ReviewSession()
                {
                    SessionStart = DateTimeOffset.Now,
                    RegisteredUser = registedUser
                };

                db.ReviewSessions.Add(newSession);
                db.SaveChanges();
            }

            var replyMessages = new List<string>()
            {
                "Good luck!",
                "Happy reviewing!",
                "Don't get lost in the queue!",
                "Watch out for audits!",
                "May the Vote be with you!"
            };

            chatRoom.PostReplyOrThrow(userMessage, replyMessages.PickRandom());
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"(?:i'?m )?start(ing|ed)(?: now)?";
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
