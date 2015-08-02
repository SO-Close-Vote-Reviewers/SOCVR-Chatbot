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

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Panic : UserCommand
    {
        public override string GetActionDescription()
        {
            return "A \"toy command\" for posting an appropriate gif.";
        }

        public override string GetActionName()
        {
            return "Panic";
        }

        public override string GetActionUsage()
        {
            return "panic!!!";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            chatRoom.PostMessageOrThrow("http://rack.0.mshcdn.com/media/ZgkyMDEzLzA2LzE4LzdjL0JlYWtlci4zOWJhOC5naWYKcAl0aHVtYgkxMjAweDk2MDA-/4a93e3c4/4a4/Beaker.gif");
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"\spanic[!.]*$";
        }
    }
}
