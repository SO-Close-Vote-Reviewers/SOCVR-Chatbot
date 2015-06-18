﻿/*
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





using TCL.Extensions;
using CVChatbot.Bot.ChatbotActions.Commands;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    /// <summary>
    /// For when a person does 40 review items and has ran out of review actions.
    /// </summary>
    public class OutOfReviewActions : EndingSessionTrigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var itemsReviewed = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            var sucessful = EndSession(incommingChatMessage, chatRoom, itemsReviewed, roomSettings);

            if (sucessful)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Thanks for reviewing! To see more information use the command `{0}`."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionStats>()));
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:> +)?thank you for reviewing (\d+) close votes today; come back in ([\w ]+) to continue reviewing\.$";
        }

        public override string GetActionName()
        {
            return "Out of Review Actions";
        }

        public override string GetActionDescription()
        {
            return null;
        }

        public override string GetActionUsage()
        {
            return null;
        }
    }
}
