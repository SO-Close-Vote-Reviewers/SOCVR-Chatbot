using System.Text.RegularExpressions;
using SOCVR.Chatbot.Bot.ChatbotActions.Commands;
using TCL.Extensions;

namespace SOCVR.Chatbot.Bot.ChatbotActions.Triggers
{
    public class OutOfCloseVotes : EndingSessionTrigger
    {
        private Regex ptn = new Regex(@"^(?:> +)?you have no more close votes today; come back in (\d+) hours\.?$", RegexObjOptions);

        public override string ActionDescription => null;

        public override string ActionName => "Out of Close Votes";

        public override string ActionUsage => null;

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var success = EndSession(incommingChatMessage, chatRoom, null, roomSettings);

            if (success)
            {
                var message = "The review session has been marked as completed. To set the number of items you reviewed use the command `{0}`"
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>());
                chatRoom.PostReplyOrThrow(incommingChatMessage, message);
            }
        }
    }
}
