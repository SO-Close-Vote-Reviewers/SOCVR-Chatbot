using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    public class Status : UserCommand
    {
        private Regex ptn = new Regex(@"^((program|chatbot|bot|what'?s your) )?status(\?)?$", RegexObjOptions);

        public override string ActionDescription =>
            "Tests if the chatbot is alive and shows simple info about it.";

        public override string ActionName => "Status";

        public override string ActionUsage => "status";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Everyone;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
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
