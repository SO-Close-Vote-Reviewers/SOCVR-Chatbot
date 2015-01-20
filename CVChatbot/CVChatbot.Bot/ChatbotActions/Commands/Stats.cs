using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Stats : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load("http://stackoverflow.com/review/close/stats");

            var statsTable = doc.DocumentNode
                .Descendants("table")
                .Where(x => x.Attributes["class"] != null)
                .Where(x => x.Attributes["class"].Value == "task-stat-table")
                .Single();

            var needReview = statsTable
                .Descendants("td")
                .ElementAt(0)
                .Element("a")
                .InnerText;

            var reviewsToday = statsTable
                .Descendants("td")
                .ElementAt(1)
                .Element("div")
                .InnerText;

            var allTime = statsTable
                .Descendants("td")
                .ElementAt(2)
                .Element("div")
                .InnerText;

            var message = new[]
            {
                "{0} need review".FormatInline(needReview),
                "{0} reviews today".FormatInline(reviewsToday),
                "{0} reviews all-time".FormatInline(allTime),
            }
            .ToCSV(Environment.NewLine);

            chatRoom.PostMessage(message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^stats$";
        }

        public override string GetActionName()
        {
            return "Stats";
        }

        public override string GetActionDescription()
        {
            return "Shows the stats at the top of the /review/close/stats page";
        }

        public override string GetActionUsage()
        {
            return "stats";
        }
    }
}
