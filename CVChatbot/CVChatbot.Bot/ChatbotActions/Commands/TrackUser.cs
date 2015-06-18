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
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class TrackUser : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var userIdToAdd = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            DatabaseAccessor da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            var existingUser = da.GetRegisteredUserByChatProfileId(userIdToAdd);

            if (existingUser != null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "That user is already in the system!");
                return;
            }

            da.AddUserToRegisteredUsersList(userIdToAdd);

            var chatUser = chatRoom.GetUser(userIdToAdd);
            chatRoom.PostReplyOrThrow(incommingChatMessage, "Ok, I added {0} ({1}) to the tracked users list."
                .FormatInline(chatUser.Name, chatUser.ID));
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Owner;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:add|track) user (\d+)$";
        }

        public override string GetActionName()
        {
            return "Add user";
        }

        public override string GetActionDescription()
        {
            return "Adds the user to the registered users list.";
        }

        public override string GetActionUsage()
        {
            return "(add | track) user <chat id>";
        }
    }
}
