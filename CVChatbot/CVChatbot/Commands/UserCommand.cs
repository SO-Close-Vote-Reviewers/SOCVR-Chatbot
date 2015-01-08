using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Represents a commands that a user types into chat to control the chat bot.
    /// </summary>
    public abstract class UserCommand
    {
        public abstract Task<bool> DoesInputTriggerCommandAsync(Message userMessage);

        public abstract Task RunCommandAsync(Message userMessage, Room chatRoom); //need to pass in something to let it talk to the chat room
    }
}
