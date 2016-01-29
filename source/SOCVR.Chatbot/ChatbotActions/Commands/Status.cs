using SOCVR.Chatbot.Database;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class Status : UserCommand
    {
        public override string ActionDescription =>
            "Tests if the chatbot is alive and shows simple info about it.";

        public override string ActionName => "Status";

        public override string ActionUsage => "status";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^((program|chatbot|bot|what'?s your) )?status(\?)?$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            var message = $"SOCVR ChatBot version {version}, running for {elapsedTime.ToUserFriendlyString()}.";

            chatRoom.PostMessageOrThrow(message);
        }
    }
}
