using CsQuery;
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
            var doc = CQ.CreateFromUrl("http://stackoverflow.com/review/close/stats");
            var statsTable = doc.Find("table.task-stat-table");
            var cells = statsTable.Find("td");

            var needReview = cells
                .ElementAt(0)
                .FirstElementChild
                .InnerText;

            var reviewsToday = cells
                .ElementAt(1)
                .FirstElementChild
                .InnerText;

            var allTime = cells
                .ElementAt(2)
                .FirstElementChild
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
