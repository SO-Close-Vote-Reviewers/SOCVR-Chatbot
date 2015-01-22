using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Status : UserCommand
    {
        public override void RunAction(Message userMessage, Room chatRoom)
        {
            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            string message = "SOCVR ChatBot version {0}, running for {1}."
                .FormatInline(version, elapsedTime.ToUserFriendlyString());

            chatRoom.PostMessageOrThrow(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^((program|chatbot|bot|what's your) )?status(\?)?$";
        }

        public override string GetActionName()
        {
            return "Status";
        }

        public override string GetActionDescription()
        {
            return "tests if the chatbot is alive and shows simple info about it";
        }

        public override string GetActionUsage()
        {
            return "status";
        }
    }
}
