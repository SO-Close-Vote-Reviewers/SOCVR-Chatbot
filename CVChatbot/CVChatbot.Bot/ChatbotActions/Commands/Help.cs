using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Help : UserCommand
    {
        public override void RunAction(Message userMessage, Room chatRoom)
        {
            string message = "This is a chat bot for the SO Close Vote Reviewers chat room, developed by [gunr2171](http://stackoverflow.com/users/1043380/gunr2171). " +
                "For more information see the [github page](https://github.com/gunr2171/SOCVR-Chatbot). " +
                "Reply with `{0}` to see a list of commands."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>());
            chatRoom.PostMessage(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^help$";
        }

        public override string GetActionName()
        {
            return "Help";
        }

        public override string GetActionDescription()
        {
            return "Prints info about this software";
        }

        public override string GetActionUsage()
        {
            return "help";
        }
    }
}
