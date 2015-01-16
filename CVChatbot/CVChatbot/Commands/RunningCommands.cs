using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Commands
{
    public class RunningCommands : UserCommand
    {

        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            throw new NotImplementedException();
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            throw new NotImplementedException();
        }

        public override string GetHelpText()
        {
            throw new NotImplementedException();
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            throw new NotImplementedException();
        }
    }
}
