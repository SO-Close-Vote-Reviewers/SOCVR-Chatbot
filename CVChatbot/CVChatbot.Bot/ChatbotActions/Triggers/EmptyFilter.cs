using System.Linq;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.Core.Database;

namespace SOCVR.Chatbot.Core.ChatbotActions.Triggers
{
    public class EmptyFilter : Trigger
    {
        private Regex ptn = new Regex("^(?:> +)?there are no items for you to review, matching the filter \"(?:[A-z-, ']+; )?([\\S ]+)\"");
        private Regex tagMatchingPattern = new Regex(@"\[(\S+?)\] ?", RegexObjOptions);

        public override string ActionDescription => null;

        public override string ActionName => "Empty Filter";

        public override string ActionUsage => null;

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            // First, get the tags that were used.
            string tags = RegexMatchingObject
                    .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                    .Groups[1]
                    .Value;

            // Split out tags.
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
    }
}
