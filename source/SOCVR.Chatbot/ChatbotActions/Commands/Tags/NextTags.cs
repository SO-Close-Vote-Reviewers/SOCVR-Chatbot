using System.Linq;
using SOCVR.Chatbot.Configuration;
using SOCVR.Chatbot.Sede;
using TCL.Extensions;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tags
{
    internal class NextTags : UserCommand
    {
        public override string ActionDescription =>
            "Displays the first X tags from the SEDE query to focus on.";

        public override string ActionName => "Next Tags";

        public override string ActionUsage => "next [#] tags";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => @"^next(?: (\d{1,9}))? tags$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            // First, get the number in the command
            var tagsToFetchArgument = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Value
                .Parse<int?>();

            int tagsToFetch;
            var maxTagsToFetch = ConfigurationAccessor.MaxFetchTags;

            if (tagsToFetchArgument != null)
            {
                if (tagsToFetchArgument <= 0)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "I can't fetch zero tags or a negative number of tags! Please use a number between 1 and {0}."
                        .FormatInline(maxTagsToFetch));
                    return;
                }

                if (tagsToFetchArgument > maxTagsToFetch)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "Sorry, that's too many tags for me. Please choose a number between 1 and {0}"
                        .FormatInline(maxTagsToFetch));
                    return;
                }

                tagsToFetch = tagsToFetchArgument.Value;
            }
            else
            {
                tagsToFetch = ConfigurationAccessor.DefaultNextTagCount;
            }

            var tags = SedeAccessor.GetTags(chatRoom, ConfigurationAccessor.LoginEmail, ConfigurationAccessor.LoginPassword);

            if (tags == null)
            {
                chatRoom.PostReplyOrThrow(incomingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            var tagString = tags
                .Take(tagsToFetch)
                .Select(x => $"[tag:{x.Key}] {x.Value}")
                .ToCSV(", ");

            var message = $"The next {tagsToFetch} tags are: {tagString}";
            chatRoom.PostReplyOrThrow(incomingChatMessage, message);
        }
    }
}
