using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Admin
{
    internal class AddReview : UserCommand
    {
        public override string ActionDescription => "Manually adds a review to a user. Should only be used for testing.";

        public override string ActionName => "Add Review";

        public override string ActionUsage => "add review [review id] [user id]";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        protected override string RegexMatchingPattern => @"^add review (\d+) (\d+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            throw new NotImplementedException();
        }
    }
}
