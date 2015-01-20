using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class UserCommand : ChatbotAction
    {
        protected override sealed string GetMessageContentsReadyForRegexParsing(Message incommingMessage)
        {
            return incommingMessage.Content
                //.GetContentsWithStrippedMentions()
                .Trim();
        }

        protected override bool GetMessageIsReplyToChatbotRequiredValue()
        {
            return true; //if you want to run a User Command it must be a reply to the chatbot.
        }
    }
}
