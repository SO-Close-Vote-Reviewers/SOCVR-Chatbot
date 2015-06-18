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





using CVChatbot.Bot.Database;
using System;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class EndSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var latestSession = da.GetLatestSessionForUser(incommingChatMessage.Author.ID);

            if (latestSession == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "I don't have any review session for you on record. Use the `{0}` command to tell me you are starting a review session."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<StartingSession>()));
                return;
            }

            if (latestSession.SessionEnd != null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Your latest review session has already been completed. Use the command `{0}` to see more information."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()));
                return;
            }

            // Else, lastestSession is open.

            da.SetSessionEndTs(latestSession.Id, DateTimeOffset.Now);

            chatRoom.PostReplyOrThrow(incommingChatMessage, "I have forcefully ended your last session. To see more details use the command `{0}`. "
                .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()) +
                "In addition, the number of review items is most likely not set, use the command `{0}` to fix that."
                .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>()));

        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(end|done( with)?) (my )?(session|review(s|ing))( pl(ease|[sz]))?$";
        }

        public override string GetActionName()
        {
            return "End Session";
        }

        public override string GetActionDescription()
        {
            return "If a user has an open review session this command will force end that session.";
        }

        public override string GetActionUsage()
        {
            return "end session";
        }
    }
}
