using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CVChatbot.Model;

namespace CVChatbot.Commands
{
    public class EndSession : UserCommand
    {
        protected override string GetMatchPattern()
        {
            return @"^(end|done( with)?) (my )?(session|review(s|ing))$";
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                //first, check if latest session is an open session

                var latestSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                if (latestSession == null)
                {
                    chatRoom.PostReply(userMessage, "I don't have any review session for you on record. Use the `starting` command to tell me you are starting a review session.");
                    return;
                }

                if (latestSession.SessionEnd != null)
                {
                    chatRoom.PostReply(userMessage, "Your latest review session has already been completed. Use the command `last session stats` to see more information.");
                    return;
                }

                //else, lastestSession is open

                latestSession.SessionEnd = DateTimeOffset.Now;
                db.SaveChanges();

                chatRoom.PostReply(userMessage, "I have forcefully ended your last session. To see more details use the command `last session stats`. " +
                    "In addition, the number of review items is most likely not set, use the command `last session edit count <new count>` to fix that.");
            }
        }

        public override string GetCommandName()
        {
            return "End Session";
        }

        public override string GetCommandDescription()
        {
            return "If a user has an open review session this command will force end that session.";
        }

        public override string GetCommandUsage()
        {
            return "end session";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
