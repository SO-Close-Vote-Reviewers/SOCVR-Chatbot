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
            return "who ...";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            var phrases = new[]
            {
                "{0}.",
                "*points finger at {0}*",
                "*looks at {0}*",
                "Blame {0}!",
                "It's definitely {0}.",
                "...{0}.",
                "I'm guessing it's {0}.",
                "Smells like {0}...",
                "It's {0}!",
                "Either {0} or {1}...",
                "{0} and {1} both look suspicious...",
                "It's {0}! Blame {0}! No, wait. It's {1}!",
                "{0} and {1}.",
                "{0} or {1}.",
                "Everyone in the room, except {0}.",
                "Everyone in the room, except {0} and {1}.",
                "Blame {0} and {1}!",
                "*{0} secretly thinks it's {1}*",
                "Jon Skeet",
                "Shog"
            };
            var users = chatRoom.GetCurrentUsers();
            var userX = users.PickRandomTheRightWay().GetChatFriendlyUsername();
            var userY = users.PickRandomTheRightWay().GetChatFriendlyUsername();
            while (userX == userY)
            {
                userY = users.PickRandomTheRightWay().GetChatFriendlyUsername();
            }
            var message = phrases.PickRandomTheRightWay().FormatInline(userX, userY);

            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(blame\??|(wh(o|ich (user|(one )?(of us|here)))))";
        }
    }
}
