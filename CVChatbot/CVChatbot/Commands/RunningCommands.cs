using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Commands
{
    public class RunningCommands : UserCommand
    {
        protected override string GetMatchPattern()
        {
            return @"^running commands$";
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            throw new NotImplementedException();
        }

        public override string GetCommandName()
        {
            return "Running Commands";
        }

        public override string GetCommandDescription()
        {
            return "Displays a list of all commands that the chat bot is currently running";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }
    }
}
