using CVChatbot.Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Shows which tags have been reported a cleared by multiple people.
    /// </summary>
    public class CompletedTags : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var thresholdInCommand =  GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(userMessage))
                .Groups[1]
                .Value
                .Parse<int?>();

            if (thresholdInCommand != null && thresholdInCommand <= 0)
            {
                chatRoom.PostReplyOrThrow(userMessage, "Minimum person threshold must be greater or equal to 1.");
                return;
            }

            var defaultThreshold = 3; // For now I'm hard-coding this.

            var peopleThreshold = thresholdInCommand ?? defaultThreshold; // Take the one in the command, or the default if the command one is not given.
            var usingDefault = thresholdInCommand == null;

            using (var db = new CVChatBotEntities())
            {
                var groupedTags = db.NoItemsInFilterEntries
                    .GroupBy(x => x.TagName)
                    .Where(x => x.Count() >= peopleThreshold)
                    .Select(x => new
                    {
                        TagName = x.Key,
                        LastTimeEntered = x.Max(y => y.EntryTs),
                        Count = x.Count()
                    })
                    .OrderByDescending(x => x.LastTimeEntered)
                    .Take(10)
                    .ToList();

                var headerMessage = "Showing the latest 10 tags that have been cleared by at least {0} people."
                .FormatInline(peopleThreshold);

                if (usingDefault)
                {
                    headerMessage += " To give a different threshold number, use the command `{0}`."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<CompletedTags>());
                }

                string dataMessage;

                if (groupedTags.Any())
                {
                    dataMessage = groupedTags
                        .ToStringTable(new[] { "Tag Name", "Count", "Latest Time Cleared" },
                            (x) => x.TagName,
                            (x) => x.Count,
                            (x) => x.LastTimeEntered.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));
                }
                else
                {
                    dataMessage = "    There are no entries that match that request!";
                }

                chatRoom.PostReplyOrThrow(userMessage, headerMessage);
                chatRoom.PostMessageOrThrow(dataMessage);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^completed tags(?: min (\d+))?$";
        }

        public override string GetActionName()
        {
            return "Completed Tags";
        }

        public override string GetActionDescription()
        {
            return "Shows the latest tags that have been completed by multiple people.";
        }

        public override string GetActionUsage()
        {
            return "completed tags [min <#>]";
        }
    }
}
