using SOCVR.Chatbot.Database;
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using LibGit2Sharp;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
{
    internal class Status : UserCommand
    {
        private readonly Repository repo;

        public override string ActionDescription =>
            "Tests if the chatbot is alive and shows simple info about it.";

        public override string ActionName => "Status";

        public override string ActionUsage => "status";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^status\??$";


        public Status()
        {
            var repoDir = SearchForRepo();
            repo = new Repository(repoDir);
        }

        ~Status()
        {
            repo?.Dispose();
        }


        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;
            var sha = repo.Head.Tip.Sha.Substring(0, 8);
            var branch = repo.Head.FriendlyName;
            var message = $"SOCVR Chatbot {branch} at `{sha}`, running for {elapsedTime.ToUserFriendlyString()}.";

            chatRoom.PostMessageOrThrow(message);
        }

        private string SearchForRepo(string curPath = ".")
        {
            var path = "";
            var dirs = Directory.EnumerateDirectories(curPath);

            foreach (var dir in dirs)
            {
                if (Path.GetDirectoryName(dir) == ".git")
                {
                    return dir;
                }
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return SearchForRepo("..");
            }

            return Directory.GetParent(path).FullName;
        }
    }
}
