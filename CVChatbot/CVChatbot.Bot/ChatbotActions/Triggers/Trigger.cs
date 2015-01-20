using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    public abstract class Trigger : ChatbotAction
    {
        protected override sealed bool GetMessageIsReplyToChatbotRequiredValue()
        {
            return false; //if you are running a trigger then it can't be a reply to the chatbot
        }

        protected override string GetMessageContentsReadyForRegexParsing(ChatExchangeDotNet.Message incommingMessage)
        {
            return incommingMessage.Content
                .Trim();
        }
    }
}
