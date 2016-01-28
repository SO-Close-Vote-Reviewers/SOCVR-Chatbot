﻿using System.Text.RegularExpressions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class StopBot : UserCommand
    {
        private Regex ptn = new Regex("^(stop( bot)?|die|shutdown)$", RegexObjOptions);

        public override string ActionDescription =>
            "The bot will leave the chat room and quit the running application.";

        public override string ActionName => "Stop Bot";

        public override string ActionUsage => "stop bot";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Owner;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom) => 
            chatRoom.PostReplyOrThrow(incommingChatMessage, "I'm shutting down...");
    }
}
