using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using System.Text.RegularExpressions;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Represents a commands that a user types into chat to control the chat bot.
    /// </summary>
    public abstract class UserCommand : ChatbotAction
    {
        protected abstract string GetMatchPattern();

        /// <summary>
        /// Takes a user message and return the message contents stripped of "mentions", ToLowers it, and trims it.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        protected string GetMessageContentsReadyForRegexParsing(Message userMessage)
        {
            return userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();
        }

        protected Regex GetRegexMatchingObject()
        {
            return new Regex(GetMatchPattern());
        }

        public bool DoesInputTriggerCommand(Message userMessage)
        {
            return GetRegexMatchingObject().IsMatch(GetMessageContentsReadyForRegexParsing(userMessage));
        }

        public abstract void RunCommand(Message userMessage, Room chatRoom);

        public abstract string GetCommandName();

        public abstract string GetCommandDescription();

        public abstract string GetCommandUsage();
    }
}
