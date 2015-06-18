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
    public class Stats : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var sa = new CloseQueueStatsAccessor();
            var message = sa.GetOverallQueueStats();

            chatRoom.PostMessageOrThrow(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^(close vote )?stats( (please|pl[sz]))?$";
        }

        public override string GetActionName()
        {
            return "Stats";
        }

        public override string GetActionDescription()
        {
            return "Shows the stats at the top of the /review/close/stats page.";
        }

        public override string GetActionUsage()
        {
            return "stats";
        }
    }
}
