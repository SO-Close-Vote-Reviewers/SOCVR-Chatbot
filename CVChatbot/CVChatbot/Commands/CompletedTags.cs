using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using CVChatbot.Model;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Shows which tags have been reported a cleared by multiple people
    /// </summary>
    public class CompletedTags : UserCommand
    {
        string matchPatternText = @"^completed tags(?: min (\d+))?$";

        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);

            var message = userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();

            return matchPattern.IsMatch(message);
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            Regex matchPattern = new Regex(matchPatternText);

            var message = userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();

            var thresholdInCommand = matchPattern.Match(message).Groups[1].Value.Parse<int?>();

            if (thresholdInCommand != null && thresholdInCommand <= 0)
            {
                chatRoom.PostReply(userMessage, "Minimum person threshold must be greater or equal to 1.");
                return;
            }

            var defaultThreshold = SettingsAccessor.GetSettingValue<int>("DefaultCompletedTagsPeopleThreshold");

            var peopleThreshold = thresholdInCommand ?? defaultThreshold; //take the one in the command, or the default if the command one is not given
            var usingDefault = thresholdInCommand == null;

            using (SOChatBotEntities db = new SOChatBotEntities())
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

                string headerMessage = "Showing the latest 10 tags that have been cleared by at least {0} people."
                .FormatInline(peopleThreshold);

                if (usingDefault)
                {
                    headerMessage += " To give a different threshold number, use the command `completed tags min <#>`.";
                }

                string dataMessage;

                if (groupedTags.Any())
                {
                    dataMessage = groupedTags
                        .Select(x => "    {0}| cleared by {1} people, last cleared {2}"
                            .FormatInline(
                                x.TagName.PadRight(10),
                                x.Count,
                                x.LastTimeEntered.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'")))
                        .ToCSV(Environment.NewLine);
                }
                else
                {
                    dataMessage = "    There are no entries that match that request!";
                }

                chatRoom.PostReply(userMessage, headerMessage);
                chatRoom.PostMessage(dataMessage);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        public override string GetHelpText()
        {
            return "completed tags [min <#>] - shows the latest tags that have been completed by multiple people.";
        }
    }
}
