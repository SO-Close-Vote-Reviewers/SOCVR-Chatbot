using SOCVR.Chatbot.Database;
using System;
using System.IO;
using LibGit2Sharp;

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
            var repoDir = SearchForRepo();
            using (var repo = new Repository(repoDir))
            {
                var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;
                var sha = repo.Head.Tip.Sha.Substring(0, 8);
                var branch = repo.Head.FriendlyName;
                var message = $"SOCVR Chatbot {branch} at `{sha}`, running for {elapsedTime.ToUserFriendlyString()}.";

                chatRoom.PostMessageOrThrow(message);
            }
        }

        private string SearchForRepo(string curPath = ".")
        {
            var path = "";
            var dirs = Directory.EnumerateDirectories(curPath);

            foreach (var dir in dirs)
            {
                if (Path.GetFileName(dir) == ".git")
                {
                    return dir;
                }
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return SearchForRepo(Directory.GetParent(curPath).FullName);
            }

            return path;
        }
    }
}
