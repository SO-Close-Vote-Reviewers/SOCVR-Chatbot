using System;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.Core.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.Core.ChatbotActions.Commands
{
    public class CurrentSession : UserCommand
    {
        private Regex ptn = new Regex(@"^(do i have an? |what is my )?(current|active|review)( review)? session( going( on)?)?\??$", RegexObjOptions);

        public override string ActionDescription =>
            "Tells if the user has an open session or not, and when it started.";

        public override string ActionName => "Current Session";

        public override string ActionUsage => "current session";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var currentSessionStartTs = da.GetCurrentSessionStartTs(incommingChatMessage.Author.ID);

            if (currentSessionStartTs == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "You don't have an ongoing review session on record.");
            }
            else
            {
                var deltaTimeSpan = DateTimeOffset.Now - currentSessionStartTs.Value;
                var formattedStartTs = currentSessionStartTs.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

                var message = "Your current review session started {0} ago at {1}"
                    .FormatInline(deltaTimeSpan.ToUserFriendlyString(), formattedStartTs);

                chatRoom.PostReplyOrThrow(incommingChatMessage, message);
            }
        }
    }
}
