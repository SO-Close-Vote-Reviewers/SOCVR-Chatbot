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





namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StopBot : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            chatRoom.PostReplyOrThrow(incommingChatMessage, "I'm shutting down...");
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Owner;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^stop bot$";
        }

        public override string GetActionName()
        {
            return "Stop Bot";
        }

        public override string GetActionDescription()
        {
            return "The bot will leave the chat room and quit the running application.";
        }

        public override string GetActionUsage()
        {
            return "stop bot";
        }
    }
}
