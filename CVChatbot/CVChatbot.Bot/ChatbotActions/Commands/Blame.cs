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





using ChatExchangeDotNet;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Blame : UserCommand
    {
        public override string GetActionDescription()
        {
            return "A \"toy command\" for blaming a chat room user.";
        }

        public override string GetActionName()
        {
            return "Blame";
        }

        public override string GetActionUsage()
        {
            return "who will ...";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            var phrases = new[]
            {
                "*points finger at X*",
                "*looks at X*",
                "Blame X!",
                "It's definitely X.",
                "...X.",
                "I'm guessing it's X.",
                "Smells like X...",
                "It's X!",
                "Either X or Y...",
                "X and Y both look suspicious...",
                "It's X! Blame X! No, wait. It's Y!",
                "X and Y.",
                "Everyone in this room, except X.",
                "Everyone in this room, except X and Y.",
                "Blame X and Y!",
                "*X secretly thinks it's Y*"
            };
            var users = chatRoom.GetPingableUsers();
            var userX = users.PickRandom();
            var userY = users.PickRandom();
            while (userX == userY)
            {
                userY = users.PickRandom();
            }
            var message = phrases.PickRandom().Replace("X", userX.Name).Replace("Y", userY.Name);

            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(blame|(wh(o|ich (user|one (of us|here)))).*)$";
        }
    }
}
