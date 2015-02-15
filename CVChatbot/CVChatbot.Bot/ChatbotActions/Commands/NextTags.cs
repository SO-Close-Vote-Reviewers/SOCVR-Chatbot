using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

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
            return "next <#> tags";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // First, get the number in the command
            var tagsToFetch = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            if (tagsToFetch <= 0)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "I can't fetch zero tags or a negative number of tags! Please use a number between 1 and {0}."
                    .FormatInline(roomSettings.MaxTagsToFetch));
                return;
            }

            if (tagsToFetch > roomSettings.MaxTagsToFetch)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Sorry, that's too many tags for me. Please choose a number between 1 and {0}"
                    .FormatInline(roomSettings.MaxTagsToFetch));
                return;
            }

            var tags = SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            var tagString = tags
                .Take(tagsToFetch)
                .Select(x => "[tag:{0}] `{1}`".FormatInline(x.Key, x.Value))
                .ToCSV(", ");

            var message = "The next {0} tags are: {1}".FormatInline(tagsToFetch, tagString);
            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^next (\d+) tags$";
        }
    }
}
