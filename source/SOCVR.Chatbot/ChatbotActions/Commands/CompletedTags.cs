using System.Linq;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.Bot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    /// <summary>
    /// Shows which tags have been reported a cleared by multiple people.
    /// </summary>
    public class CompletedTags : UserCommand
    {
        private Regex ptn = new Regex(@"^completed tags(?: min (\d+))?$", RegexObjOptions);

        public override string ActionDescription =>
            "Shows the latest tags that have been completed by multiple people.";

        public override string ActionName => "Completed Tags";

        public override string ActionUsage => "completed tags [min <#>]";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var thresholdInCommand =  RegexMatchingObject
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int?>();

            if (thresholdInCommand != null && thresholdInCommand <= 0)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "Minimum person threshold must be greater or equal to 1.");
                return;
            }

            var defaultThreshold = roomSettings.DefaultCompletedTagsPeopleThreshold;

            var peopleThreshold = thresholdInCommand ?? defaultThreshold; // Take the one in the command, or the default if the command one is not given.
            var usingDefault = thresholdInCommand == null;

            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var completedTagsData = da.GetCompletedTags(peopleThreshold, 10); // 10 is hard coded for now, could be changed later

            var headerMessage = "Showing the latest 10 tags that have been cleared by at least {0} {1}."
                .FormatInline(peopleThreshold, peopleThreshold != 1 ? "people" : "person");

            if (usingDefault)
            {
                headerMessage += " To give a different threshold number, use the command `{0}`."
                    .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<CompletedTags>());
            }

            string dataMessage;

            if (completedTagsData.Any())
            {
                dataMessage = completedTagsData
                    .ToStringTable(new[] { "Tag Name", "Count", "Latest Time Cleared" },
                        x => x.TagName,
                        x => x.PeopleWhoCompletedTag,
                        x => x.LastEntryTs.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));
            }
            else
            {
                dataMessage = "    There are no entries that match that request!";
            }

            chatRoom.PostReplyOrThrow(incommingChatMessage, headerMessage);
            chatRoom.PostMessageOrThrow(dataMessage);
        }
    }
}
