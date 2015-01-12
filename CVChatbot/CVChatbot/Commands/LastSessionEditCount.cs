using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Used for editing the last completed session's review count.
    /// </summary>
    public class LastSessionEditCount : UserCommand
    {
        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            throw new NotImplementedException();
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            throw new NotImplementedException();
        }
    }
}
