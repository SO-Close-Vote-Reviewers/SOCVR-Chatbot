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
using System.Linq;
using System.Text.RegularExpressions;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    public class EmptyFilter : Trigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // First, get the tags that were used.
            string tags = GetRegexMatchingObject()
                    .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                    .Groups[1]
                    .Value;

            // Split out tags.
            var tagMatchingPattern = new Regex(@"\[(\S+?)\] ?", RegexOptions.CultureInvariant);
            var parsedTagNames = tagMatchingPattern.Matches(incommingChatMessage.Content.ToLower())
                .Cast<Match>()
                .Select(x => x.Groups[1].Value)
                .ToList();

            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

            // Save the tags to the database.
            foreach (var tagName in parsedTagNames)
            {
                da.InsertNoItemsInFilterRecord(incommingChatMessage.Author.ID, tagName);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^(?:> +)?there are no items for you to review, matching the filter \"(?:[A-z-, ']+; )?([\\S ]+)\"";
        }

        public override string GetActionName()
        {
            return "Empty Filter";
        }

        public override string GetActionDescription()
        {
            return null;
        }

        public override string GetActionUsage()
        {
            return null;
        }
    }
}
