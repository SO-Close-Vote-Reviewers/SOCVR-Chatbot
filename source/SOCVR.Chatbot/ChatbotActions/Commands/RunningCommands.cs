using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using SOCVR.Chatbot.ChatRoom;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    internal class RunningCommands : UserCommand
    {
        public override string ActionDescription =>
            "Displays a list of all commands that the chat bot is currently running.";

        public override string ActionName => "Running Commands";

        public override string ActionUsage => "running commands";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => @"^(show (a |me )?)?(list of |the )?running (commands|actions)( (please|pl[sz]))?$";

        public override void RunAction(Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var runningCommands = RunningChatbotActionsManager.GetRunningChatbotActions();
            var now = DateTimeOffset.Now;

            var tableMessage = runningCommands
                .Select(x => new
                {
                    Command = x.ChatbotActionName,
                    ForUser = $"{x.RunningForUserName} ({x.RunningForUserId})",
                    Started = (now - x.StartTs).ToUserFriendlyString() + " ago",
                })
                .ToStringTable(new[] { "Command", "For User", "Started" },
                    x => x.Command,
                    x => x.ForUser,
                    x => x.Started);

            chatRoom.PostReplyOrThrow(incomingChatMessage, "The following is a list of commands that I'm currently running:");
            chatRoom.PostMessageOrThrow(tableMessage);
        }
    }
}
