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





namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartingSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var message = "You don't need to run this command anymore! When you start reviewing I should notice it and record the start of the record.";
            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:i'?m )?start(ing|ed)(?: now)?$";
        }

        public override string GetActionName()
        {
            return "Starting";
        }

        public override string GetActionDescription()
        {
            return "Unnecessary - Informs the chatbot that you are starting a new review session.";
        }

        public override string GetActionUsage()
        {
            return "starting";
        }
    }
}
