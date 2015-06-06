﻿using System;
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
