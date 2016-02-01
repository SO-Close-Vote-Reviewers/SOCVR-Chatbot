using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tracking
{
    class OptIn : UserCommand
    {
        public override string ActionDescription => "Tells the bot to resume tracking your close vote reviewing.";

        public override string ActionName => "Opt In";

        public override string ActionUsage => "opt in";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^opt in$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            throw new NotImplementedException();
        }
    }
}
