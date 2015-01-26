using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    /// <summary>
    /// A Trigger is a message said in chat which is not a direct reply to the chatbot, but the chatbot will take action on the message.
    /// </summary>
    public abstract class Trigger : ChatbotAction
    {
        /// <summary>
        /// Hard-coded to return false.
        /// If you are running a trigger then it can't be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected override sealed bool GetMessageIsReplyToChatbotRequiredValue()
        {
            return false;
        }

        /// <summary>
        /// Takes the content from the message and trims the contents. It does not remove or replace anything within the string.
        /// </summary>
        /// <param name="incommingMessage"></param>
        /// <returns></returns>
        protected override string GetMessageContentsReadyForRegexParsing(ChatExchangeDotNet.Message incommingMessage)
        {
            return incommingMessage.Content
                .Trim();
        }
    }
}
