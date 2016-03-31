using System;
using System.Linq;
using ChatExchangeDotNet;
using SOCVR.Chatbot.ChatRoom;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
{
    internal class RunningCommands : UserCommand
    {
        public override string ActionDescription =>
            "Displays a list of all commands that the chat bot is currently running.";

        public override string ActionName => "Running Commands";

        public override string ActionUsage => "running commands";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

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

            chatRoom.PostReplyOrThrow(incomingChatMessage, "Oh, sure, you want to micromanage me now? Give me a bunch of things to do then constantly check up on me? Does that sound very fair to you? No, no it's not. You should feel ashamed of yourself.");
            chatRoom.PostMessageOrThrow(tableMessage);
        }
    }
}
