﻿using CVChatbot.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class CurrentSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var currentSessionStartTs = da.GetCurrentSessionStartTs(incommingChatMessage.Author.ID);

            if (currentSessionStartTs == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "You don't have an ongoing review session on record.");
            }
            else
            {
                var deltaTimeSpan = DateTimeOffset.Now - currentSessionStartTs.Value;
                var formattedStartTs = currentSessionStartTs.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

                var message = "Your current review session started {0} ago at {1}"
                    .FormatInline(deltaTimeSpan.ToUserFriendlyString(), formattedStartTs);

                chatRoom.PostReplyOrThrow(incommingChatMessage, message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(do i have an? |what is my )?(current|active|review)( review)? session( going( on)?)?\??$";
        }

        public override string GetActionName()
        {
            return "Current Session";
        }

        public override string GetActionDescription()
        {
            return "Tells if the user has an open session or not, and when it started.";
        }

        public override string GetActionUsage()
        {
            return "current session";
        }
    }
}
