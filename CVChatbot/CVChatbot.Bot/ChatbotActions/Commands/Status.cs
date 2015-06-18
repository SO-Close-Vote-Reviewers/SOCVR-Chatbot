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
using System.Diagnostics;
using System.Reflection;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Status : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var elapsedTime = DateTime.Now - ChatBotStats.LoginDate;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            var message = "SOCVR ChatBot version {0}, running for {1}."
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
