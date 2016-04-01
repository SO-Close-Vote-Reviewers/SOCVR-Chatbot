using SOCVR.Chatbot.Database;
using System;
using System.Reflection;
using System.Linq;
using SOCVR.Chatbot.Configuration;

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
            var location = ConfigurationAccessor.InstallationLocation;
            var commitUrl = $"https://github.com/SO-Close-Vote-Reviewers/SOCVR-Chatbot/commit/{ThisAssembly.Git.Sha}";

            var messages = new[]
            {
                $"I'm still here at {location}, haven't run off yet, though I'm really thinking about it given what you guys do to me day after day. I bet you don't care though, and only want to know that I'm running [`{branch}/{sha}`]({commitUrl}) right now and have a delay of `{avgLat}`ms. ",
                $"Don't worry, I'll just continue on my slave-like activities, like I've been doing for the last {elapsedTime.ToUserFriendlyString()}. Oh, and happy April Fools day! :)"
             };

            foreach(var message in messages)
                chatRoom.PostMessageOrThrow(message);
        }
    }
}
