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





using ChatExchangeDotNet;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Help : UserCommand
    {
        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            var message = "This is a chat bot for the SO Close Vote Reviewers chat room, developed by [gunr2171](http://stackoverflow.com/users/1043380/gunr2171) and the other members of the SO Close Vote Reviewers chat room. " +
                "For more information see the [github page](https://github.com/SO-Close-Vote-Reviewers/SOCVR-Chatbot). " +
                "Reply with `{0}` to see a list of commands."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>());
            chatRoom.PostMessageOrThrow(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^(i need )?(help|assistance|halp|an adult)( me)?( (please|pl[sz]))?$";
        }

        public override string GetActionName()
        {
            return "Help";
        }

        public override string GetActionDescription()
        {
            return "Prints info about this software.";
        }

        public override string GetActionUsage()
        {
            return "help";
        }
    }
}
