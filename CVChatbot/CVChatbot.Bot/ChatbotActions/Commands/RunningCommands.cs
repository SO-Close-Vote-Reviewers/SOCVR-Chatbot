using System;
using System.Linq;
using System.Text.RegularExpressions;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class RunningCommands : UserCommand
    {
        private Regex ptn = new Regex(@"^(show (a |me )?)?(list of |the )?running (commands|actions)( (please|pl[sz]))?$", RegexObjOptions);

        public override string ActionDescription =>
            "Displays a list of all commands that the chat bot is currently running.";

        public override string ActionName => "Running Commands";

        public override string ActionUsage => "running commands";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Everyone;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
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

            chatRoom.PostReplyOrThrow(incommingChatMessage, "The following is a list of commands that I'm currently running:");
            chatRoom.PostMessageOrThrow(tableMessage);
        }
    }
}
