using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Commands
{
    public class Help : UserCommand
    {
        public override void RunCommand(Message userMessage, Room chatRoom)
        {
            string message = "This is a chat bot for the SO Close Vote Reviewers chat room, developed by [gunr2171](http://stackoverflow.com/users/1043380/gunr2171). For more information see the [github page](https://github.com/gunr2171/SOCVR-Chatbot). Reply with `commands` to see a list of commands.";
            chatRoom.PostMessage(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetMatchPattern()
        {
            return "^(i need )?(help|assistance)( me)?( (please|plz))?$";
        }

        public override string GetCommandName()
        {
            return "Help";
        }

        public override string GetCommandDescription()
        {
            return "Prints info about this software";
        }

        public override string GetCommandUsage()
        {
            return "help";
        }
    }
}
