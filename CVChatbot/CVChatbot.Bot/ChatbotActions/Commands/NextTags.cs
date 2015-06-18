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





using System.Linq;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class NextTags : UserCommand
    {
        public override string GetActionDescription()
        {
            return "Displays the first X tags from the SEDE query to focus on.";
        }

        public override string GetActionName()
        {
            return "Next Tags";
        }

        public override string GetActionUsage()
        {
            return "next [#] tags";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // First, get the number in the command
            var tagsToFetchArgument = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int?>();

            int tagsToFetch;

            if (tagsToFetchArgument != null)
            {
                if (tagsToFetchArgument <= 0)
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "I can't fetch zero tags or a negative number of tags! Please use a number between 1 and {0}."
                        .FormatInline(roomSettings.MaxTagsToFetch));
                    return;
                }

                if (tagsToFetchArgument > roomSettings.MaxTagsToFetch)
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "Sorry, that's too many tags for me. Please choose a number between 1 and {0}"
                        .FormatInline(roomSettings.MaxTagsToFetch));
                    return;
                }

                tagsToFetch = tagsToFetchArgument.Value;
            }
            else
            {
                tagsToFetch = roomSettings.DefaultNextTagCount;
            }

            var tags = SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            if (tags == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            var tagString = tags
                .Take(tagsToFetch)
                .Select(x => "[tag:{0}] `{1}`".FormatInline(x.Key, x.Value))
                .ToCSV(", ");

            var message = "The next {0} tags are: {1}".FormatInline(tagsToFetch, tagString);
            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^next(?: (\d+))? tags$";
        }
    }
}
