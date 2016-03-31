using SOCVR.Chatbot.Database;
using System.Collections.Generic;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
{
    internal class Alive : UserCommand
    {
        public override string ActionDescription => "A simple ping command to test if the bot is running.";

        public override string ActionName => "Alive";

        public override string ActionUsage => "alive";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^alive$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var responsePhrases = new List<string>()
            {
                "Nope",
                "What do you think?",
                "Well I responded, that good enough for you?",
                "I was just on my way out.",
                "Oh, you _actually_ care? Sure you do...",
                "...",
                "'alive'? You can't even be bothered to make a whole sentence. What am I to you, some sort of bot?"
            };

            var phrase = responsePhrases.PickRandom();

            chatRoom.PostReplyOrThrow(incomingChatMessage, phrase);
        }
    }
}
