using ChatExchangeDotNet;
using CVChatbot.Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

            // Save the tags to the database.
            using (var db = new CVChatBotEntities())
            {
                foreach (var tagName in parsedTagNames)
                {
                    var registedUser = db.RegisteredUsers
                        .Single(x => x.ChatProfileId == incommingChatMessage.AuthorID);

                    var newEntry = new NoItemsInFilterEntry()
                    {
                        EntryTs = DateTimeOffset.Now,
                        RegisteredUser = registedUser,
                        TagName = tagName
                    };

                    db.NoItemsInFilterEntries.Add(newEntry);
                    db.SaveChanges();
                }
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "(?:> )?there are no items for you to review, matching the filter \"(?:[A-z-, ']+; )?([\\S ]+)\"";
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
