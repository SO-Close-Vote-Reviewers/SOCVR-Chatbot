using System;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.Core.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.Core.ChatbotActions.Commands
{
    public class EndSession : UserCommand
    {
        private Regex ptn = new Regex(@"^(end|done( with)?) (my )?(session|review(s|ing))( pl(ease|[sz]))?$", RegexObjOptions);

        public override string ActionDescription =>
            "If a user has an open review session this command will force end that session.";

        public override string ActionName => "End Session";

        public override string ActionUsage => "end session";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var latestSession = da.GetLatestSessionForUser(incommingChatMessage.Author.ID);

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
    }
}
