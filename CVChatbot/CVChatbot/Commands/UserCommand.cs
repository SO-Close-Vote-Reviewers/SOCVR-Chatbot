using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Represents a commands that a user types into chat to control the chat bot.
    /// </summary>
    public abstract class UserCommand
    {
        public abstract Task<bool> DoesInputTriggerCommandAsync(/* add Message datatype here */);

        public abstract Task RunCommandAsync(); //need to pass in something to let it talk to the chat room
    }
}
