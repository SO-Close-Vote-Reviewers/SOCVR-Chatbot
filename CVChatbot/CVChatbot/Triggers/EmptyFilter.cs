using ChatExchangeDotNet;
using CVChatbot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Triggers
{
    public class EmptyFilter : Trigger
    {
        string fullTriggerMatchPattern = "(?:> )?There are no items for you to review, matching the filter \"(?:[A-z-, ']+; )?([\\S ]+)\"";

        public override bool DoesInputActivateTrigger(Message userMessage)
        {
            //this regex only checks if it follows the formula
            var matchRegex = new Regex(fullTriggerMatchPattern);

            var isMatch = matchRegex.IsMatch(userMessage.Content);
            return isMatch;
        }

        public override void RunTrigger(Message userMessage, Room chatRoom)
        {
            //first, get the tags that were used
            var overallPattern = new Regex(fullTriggerMatchPattern);
            string tags = overallPattern.Match(userMessage.Content).Groups[1].Value;

            //split out tags
            var tagMatchingPattern = new Regex(@"\[(\S+?)\] ?");
            var parsedTagNames = tagMatchingPattern.Matches(userMessage.Content)
                .Cast<Match>()
                .Select(x => x.Groups[1].Value)
                .ToList();

            //save the tags to the database
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                foreach (var tagName in parsedTagNames)
                {
                    var user = DBCommonActions.GetRegisteredUser(userMessage.AuthorID, db);
                    NoItemsInFilterEntry newEntry = new NoItemsInFilterEntry()
                    {
                        EntryTs = DateTimeOffset.Now,
                        RegisteredUser = user,
                        TagName = tagName
                    };

                    db.NoItemsInFilterEntries.Add(newEntry);
                    db.SaveChanges();
                }
            }
        }
    }
}
