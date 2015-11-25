using System;
using System.Linq;
using CsQuery;
using TCL.Extensions;

namespace SOCVR.Chatbot.Bot
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
                $"{needReview} need review",
                $"{reviewsToday} reviews today",
                $"{allTime} reviews all-time",
            }
            .ToCSV(Environment.NewLine);
            return message;
        }
    }
}
