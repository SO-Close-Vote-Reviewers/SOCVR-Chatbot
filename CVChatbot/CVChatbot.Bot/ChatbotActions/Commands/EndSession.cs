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
    public class EndSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var msg = "You don't need to run this command anymore!" +
                " Your session automatically ends when you reach either 40 CVs or the end of this UTC day.";
            chatRoom.PostReplyOrThrow(incommingChatMessage, msg);
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
            return "Deprecated - If a user has an open review session this command will force end that session.";
        }

        public override string GetActionUsage()
        {
            return "end session";
        }
    }
}
