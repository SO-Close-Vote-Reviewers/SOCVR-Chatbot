using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CVChatbot.Bot.Model;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class EndSession : UserCommand
    {
        protected override string GetRegexMatchingPattern()
        {
            return @"^(end|done( with)?) (my )?(session|review(s|ing))( pl(ease|z))?$";
        }

        public override void RunAction(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (CVChatBotEntities db = new CVChatBotEntities())
            {
                //first, check if latest session is an open session

                var latestSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                if (latestSession == null)
                {
                    chatRoom.PostReplyOrThrow(userMessage, "I don't have any review session for you on record. Use the `{0}` command to tell me you are starting a review session."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<StartingSession>()));
                    return;
                }

                if (latestSession.SessionEnd != null)
                {
                    chatRoom.PostReplyOrThrow(userMessage, "Your latest review session has already been completed. Use the command `{0}` to see more information."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()));
                    return;
                }

                //else, lastestSession is open

                latestSession.SessionEnd = DateTimeOffset.Now;
                db.SaveChanges();

                chatRoom.PostReplyOrThrow(userMessage, "I have forcefully ended your last session. To see more details use the command `{0}`. "
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()) +
                    "In addition, the number of review items is most likely not set, use the command `{0}` to fix that."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>()));
            }
        }

        public override string GetActionName()
        {
            return "End Session";
        }

        public override string GetActionDescription()
        {
            return "If a user has an open review session this command will force end that session.";
        }

        public override string GetActionUsage()
        {
            return "end session";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
