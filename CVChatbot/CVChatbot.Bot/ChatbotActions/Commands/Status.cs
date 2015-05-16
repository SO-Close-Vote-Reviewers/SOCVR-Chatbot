using ChatExchangeDotNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Status : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;
            string gitCommitId = null;

            if (!roomSettings.GitRootDirectoryPath.IsNullOrWhiteSpace())
            {
                try
                {
                    using (var repo = new Repository(roomSettings.GitRootDirectoryPath))
                    {
                        //take the first 6 chars from the latest git commit
                        gitCommitId = repo.Commits.First().Sha.Take(6).ToCSV("");
                    }
                }
                catch(Exception ex)
                {
                    gitCommitId = "`<error on retrieval, {0}>`".FormatInline(ex.Message);
                }
            }

            string version = "";

            if (gitCommitId != null)
            {
                version = " version " + gitCommitId;
            }

            var message = "SOCVR ChatBot{0}, running for {1}."
                .FormatInline(version, elapsedTime.ToUserFriendlyString());

            chatRoom.PostMessageOrThrow(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^((program|chatbot|bot|what'?s your) )?status(\?)?$";
        }

        public override string GetActionName()
        {
            return "Status";
        }

        public override string GetActionDescription()
        {
            return "Tests if the chatbot is alive and shows simple info about it.";
        }

        public override string GetActionUsage()
        {
            return "status";
        }
    }
}
