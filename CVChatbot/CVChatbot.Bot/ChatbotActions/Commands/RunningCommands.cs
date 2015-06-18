/*
 * ChatExchange.Net. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Linq;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class RunningCommands : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var runningCommands = RunningChatbotActionsManager.GetRunningChatbotActions();
            var now = DateTimeOffset.Now;

            var tableMessage = runningCommands
                .Select(x => new
                {
                    Command = x.ChatbotActionName,
                    ForUser = "{0} ({1})".FormatInline(x.RunningForUserName, x.RunningForUserId),
                    Started = (now - x.StartTs).ToUserFriendlyString() + " ago",
                })
                .ToStringTable(new[] { "Command", "For User", "Started" },
                    x => x.Command,
                    x => x.ForUser,
                    x => x.Started);

            chatRoom.PostReplyOrThrow(incommingChatMessage, "The following is a list of commands that I'm currently running:");
            chatRoom.PostMessageOrThrow(tableMessage);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(show (a |me )?)?(list of |the )?running (commands|actions)( (please|pl[sz]))?$";
        }

        public override string GetActionName()
        {
            return "Running Commands";
        }

        public override string GetActionDescription()
        {
            return "Displays a list of all commands that the chat bot is currently running.";
        }

        public override string GetActionUsage()
        {
            return "running commands";
        }
    }
}
