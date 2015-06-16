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
