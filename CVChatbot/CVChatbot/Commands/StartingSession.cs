using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CVChatbot.Model;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class StartingSession : UserCommand
    {
        public override bool DoesInputTriggerCommand(Message userMessage)
        {
            return userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim()
                == "starting";
        }

        public override void RunCommand(Message userMessage, Room chatRoom)
        {
            var chatUser = chatRoom.GetUser(userMessage.AuthorID);

            //start a new review session
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                var registedUser = db.RegisteredUsers
                    .Single(x => x.ChatProfileId == userMessage.AuthorID);

                ReviewSession newSession = new ReviewSession()
                {
                    SessionStart = DateTimeOffset.Now,
                    RegisteredUser = registedUser
                };

                db.ReviewSessions.Add(newSession);
                db.SaveChanges();
            }

            List<string> replyMessages = new List<string>()
            {
                "Good luck!",
                "Happy reviewing!",
                "Don't get lost in the queue!",
                "Watch out for audits!",
                "May the Vote be with you!"
            };

            chatRoom.PostReply(userMessage, replyMessages.PickRandom());
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        public override string GetHelpText()
        {
            return "starting - informs the chatbot that you are starting a new review session";
        }
    }
}
