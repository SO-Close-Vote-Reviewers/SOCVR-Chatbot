using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Triggers
{
    /// <summary>
    /// Represents a message that a person types into chat that the chat bot will react on.
    /// </summary>
    public abstract class Trigger : ChatbotAction
    {
        public abstract void RunTrigger(Message userMessage, Room chatRoom);

        public override bool DoesChatMessageActivateAction(Message chatMessage)
        {
            return GetRegexMatchingObject().IsMatch(GetMessageContentsReadyForRegexParsing(chatMessage));
        }

        protected Regex GetRegexMatchingObject()
        {
            return new Regex(GetMatchPattern());
        }

        /// <summary>
        /// Takes a user message and return the message contents stripped of "mentions", ToLowers it, and trims it.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        protected string GetMessageContentsReadyForRegexParsing(Message userMessage)
        {
            return userMessage
                .GetContentsWithStrippedMentions()
                .ToLowerInvariant()
                .Trim();
        }

        protected abstract string GetMatchPattern();
    }
}
