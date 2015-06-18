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





using System.Collections.Generic;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Alive : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var responsePhrases = new List<string>()
            {
                "I'm alive and kicking!",
                "Still here you guys!",
                "I'm not dead yet!",
                "I feel... happy!",
                "I think I'll go for a walk...",
                "I don't want to go on the cart!",
                "I feel fine.",
            };

            var phrase = responsePhrases.PickRandom();

            chatRoom.PostReplyOrThrow(incommingChatMessage, phrase);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:(?:are )?you )?(alive|still there|(still )?with us)\??$";
        }

        public override string GetActionName()
        {
            return "Alive";
        }

        public override string GetActionDescription()
        {
            return "A simple ping command to test if the bot is running.";
        }

        public override string GetActionUsage()
        {
            return "alive";
        }
    }
}
