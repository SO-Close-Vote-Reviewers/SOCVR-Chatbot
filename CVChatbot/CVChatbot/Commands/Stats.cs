using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class Stats : UserCommand
    {
        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            return userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim()
                == "stats";
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
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
                .InnerText
                .Replace(",", "")
                .Parse<int>();

            var reviewsToday = statsTable
                .Descendants("td")
                .ElementAt(1)
                .Element("div")
                .InnerText
                .Replace(",", "")
                .Parse<int>();

            var message = new[]
            {
                "{0} need review".FormatInline(needReview),
                "{0} reviews today".FormatInline(reviewsToday)
            }
            .ToCSV(Environment.NewLine);

            chatRoom.PostMessage(message);
        }
    }
}
