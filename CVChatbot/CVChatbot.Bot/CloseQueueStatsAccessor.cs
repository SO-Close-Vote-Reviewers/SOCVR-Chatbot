using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

namespace CVChatbot.Bot
{
    /// <summary>
    /// Class for getting stats about the Close Vote Queue.
    /// </summary>
    public class CloseQueueStatsAccessor
    {
        public string GetOverallQueueStats()
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load("http://stackoverflow.com/review/close/stats");

            var statsTable = doc.DocumentNode
                .Descendants("table")
                .Single(x =>
                    x.Attributes["class"] != null &&
                    x.Attributes["class"].Value == "task-stat-table");

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
            return message;
        }
    }
}
