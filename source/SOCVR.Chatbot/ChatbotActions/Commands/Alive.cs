﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class Alive : UserCommand
    {
        public override string ActionDescription => "A simple ping command to test if the bot is running.";

        public override string ActionName => "Alive";

        public override string ActionUsage => "alive";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Everyone;

        protected override string RegexMatchingPattern => @"^(?:(?:are )?you )?(alive|still there|(still )?with us)\??$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var responsePhrases = new List<string>()
            {
                "I'm alive and kicking!",
                "Still here you guys!",
                "I'm not dead yet!",
                "I feel... happy!",
                "I think I'll go for a walk...",
                "I don't want to go on the cart!",
                "I feel fine.",
            };

            var phrase = responsePhrases.PickRandom();

            chatRoom.PostReplyOrThrow(incomingChatMessage, phrase);
        }
    }
}
