using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Triggers
{
    /// <summary>
    /// Represents a message that a person types into chat that the chat bot will react on.
    /// </summary>
    public abstract class Trigger
    {
        public abstract Task<bool> DoesInputActivateTriggerAsync();

        public abstract Task RunTriggerAsync();
    }
}
