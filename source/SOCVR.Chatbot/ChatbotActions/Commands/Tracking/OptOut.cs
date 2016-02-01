using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tracking
{
    class OptOut : UserCommand
    {
        public override string ActionDescription => "Tells the bot to stop tracking your close vote reviewing.";

        public override string ActionName => "Opt Out";

        public override string ActionUsage => "opt out";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^opt out$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            throw new NotImplementedException();
        }
    }
}
