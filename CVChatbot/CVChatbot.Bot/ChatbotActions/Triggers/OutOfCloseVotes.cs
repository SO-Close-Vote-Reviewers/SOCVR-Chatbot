/*
 * CVChatbot. Chatbot for the SO Close Vote Reviewers Chat Room.
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





using CVChatbot.Bot.ChatbotActions.Commands;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    public class OutOfCloseVotes : EndingSessionTrigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var success = EndSession(incommingChatMessage, chatRoom, null, roomSettings);

            if (success)
            {
                var message = "The review session has been marked as completed. To set the number of items you reviewed use the command `{0}`"
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>());
                chatRoom.PostReplyOrThrow(incommingChatMessage, message);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:> +)?you have no more close votes today; come back in (\d+) hours\.$";
        }

        public override string GetActionName()
        {
            return "Out of Close Votes";
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
