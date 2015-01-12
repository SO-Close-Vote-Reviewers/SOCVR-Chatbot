using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CVChatbot.Model;

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
                var user = DBCommonActions.GetRegisteredUser(chatUser.ID, db);

                ReviewSession newSession = new ReviewSession()
                {
                    SessionStart = DateTimeOffset.Now,
                    RegisteredUser = user
                };

                db.ReviewSessions.Add(newSession);
                db.SaveChanges();
            }
        }
    }
}
