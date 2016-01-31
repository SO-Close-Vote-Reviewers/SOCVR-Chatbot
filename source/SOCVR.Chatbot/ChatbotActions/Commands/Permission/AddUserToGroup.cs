using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    class AddUserToGroup : UserCommand
    {
        public override string ActionDescription => "Manually adds a user to the given permission group.";

        public override string ActionName => "Add User To Group";

        public override string ActionUsage => "add [user id] to [group name]";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^add (\d+) to (\w+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            //var targetUserId = 
        }
    }
}
