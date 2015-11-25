using System.Linq;
using System.Text.RegularExpressions;
using TCL.Extensions;

namespace SOCVR.Chatbot.Bot.ChatbotActions.Commands
{
    public class NextTags : UserCommand
    {
        private Regex ptn = new Regex(@"^next(?: (\d+))? tags$", RegexObjOptions);

        public override string ActionDescription =>
            "Displays the first X tags from the SEDE query to focus on.";

        public override string ActionName => "Next Tags";

        public override string ActionUsage => "next [#] tags";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // First, get the number in the command
            var tagsToFetchArgument = RegexMatchingObject
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
                .Select(x => $"[tag:{x.Key}] `{x.Value}`")
                .ToCSV(", ");

            var message = $"The next {tagsToFetch} tags are: {tagString}";
            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }
    }
}
