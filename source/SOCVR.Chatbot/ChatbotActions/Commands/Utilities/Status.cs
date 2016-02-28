using SOCVR.Chatbot.Database;
using System;
using System.Reflection;
using System.Linq;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
{
    internal class Status : UserCommand
    {
        public override string ActionDescription =>
            "Tests if the chatbot is alive and shows simple info about it.";

        public override string ActionName => "Status";

        public override string ActionUsage => "status";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^status\??$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var tracker = (UserTracking)typeof(Program).GetField("watcher", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var avgLat = Math.Round(tracker.WatchedUsers.Values.Average(x => x.DetectionLatency.TotalMilliseconds));

            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;
            var sha = ThisAssembly.Git.Sha.Substring(0, 8);
            var branch = ThisAssembly.Git.Branch;
            var message = $"SOCVR Chatbot {branch} at `{sha}`, running for {elapsedTime.ToUserFriendlyString()} (tracking latency `{avgLat}`ms).";

            chatRoom.PostMessageOrThrow(message);
        }
    }
}
