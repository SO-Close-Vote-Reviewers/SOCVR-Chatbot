using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class Status : UserCommand
    {
        public override void RunCommand(Message userMessage, Room chatRoom)
        {
            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            string message = "SOCVR ChatBot version {0}, running for {1}."
                .FormatInline(version, elapsedTime.ToUserFriendlyString());

            chatRoom.PostMessage(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetMatchPattern()
        {
            return "^status$";
        }

        public override string GetCommandName()
        {
            return "Status";
        }

        public override string GetCommandDescription()
        {
            return "tests if the chatbot is alive and shows simple info about it";
        }
    }
}
