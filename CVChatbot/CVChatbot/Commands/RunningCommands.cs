using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class RunningCommands : UserCommand
    {
        protected override string GetMatchPattern()
        {
            return @"^running commands$";
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var runningCommands = RunningCommandsManager.GetRunningCommands();
            var now = DateTimeOffset.Now;

            var tableMessage = runningCommands
                .Select(x => new
                {
                    Command = x.CommandName,
                    ForUser = "{0} ({1})".FormatInline(x.RunningForUserName, x.RunningForUserId),
                    Started = (now - x.CommandStartTs).ToUserFriendlyString() + " ago",
                })
                .ToStringTable(new string[] { "Command", "For User", "Started" },
                    x => x.Command,
                    x => x.ForUser,
                    x => x.Started);

            chatRoom.PostReply(userMessage, "The following is a list of commands that I'm currently running:");
            chatRoom.PostMessage(tableMessage);
        }

        public override string GetCommandName()
        {
            return "Running Commands";
        }

        public override string GetCommandDescription()
        {
            return "Displays a list of all commands that the chat bot is currently running";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }
    }
}
