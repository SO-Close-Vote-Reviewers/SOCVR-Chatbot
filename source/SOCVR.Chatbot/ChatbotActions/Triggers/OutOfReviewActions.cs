using System.Text.RegularExpressions;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Triggers
{
    /// <summary>
    /// For when a person does 40 review items and has ran out of review actions.
    /// </summary>
    public class OutOfReviewActions : EndingSessionTrigger
    {
        private Regex ptn = new Regex(@"^(?:> +)?thank you for reviewing (?!-)([1-9]|[1-3]\d|40) close votes today; come back in ([\w ]+) to continue reviewing\.?$", RegexObjOptions);

        public override string ActionDescription => null;

        public override string ActionName => "Out of Review Actions";

        public override string ActionUsage => null;

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var itemsReviewed = RegexMatchingObject
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            var sucessful = EndSession(incommingChatMessage, chatRoom, itemsReviewed, roomSettings);

            if (sucessful)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Thanks for reviewing! To see more information use the command `{0}`."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()));
            }
        }
    }
}
