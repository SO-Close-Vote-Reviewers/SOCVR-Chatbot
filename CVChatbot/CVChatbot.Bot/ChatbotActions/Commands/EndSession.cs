using CVChatbot.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class EndSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var latestSession = da.GetLatestSessionForUser(incommingChatMessage.AuthorID);

            if (latestSession == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any review session for you on record. Use the `{0}` command to tell me you are starting a review session."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<StartingSession>()));
                return;
            }

            if (latestSession.SessionEnd != null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Your latest review session has already been completed. Use the command `{0}` to see more information."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()));
                return;
            }

            // Else, lastestSession is open.

            da.SetSessionEndTs(latestSession.Id, DateTimeOffset.Now);

            chatRoom.PostReplyOrThrow(incommingChatMessage, "I have forcefully ended your last session. To see more details use the command `{0}`. "
                .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()) +
                "In addition, the number of review items is most likely not set, use the command `{0}` to fix that."
                .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>()));

        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(end|done( with)?) (my )?(session|review(s|ing))( pl(ease|[sz]))?$";
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
    }
}
