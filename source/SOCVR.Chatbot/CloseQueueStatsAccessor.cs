using System.Linq;
using CsQuery;
using System.Globalization;

namespace SOCVR.Chatbot
{
    /// <summary>
    /// Class for getting stats about the Close Vote Queue.
    /// </summary>
    internal class CloseQueueStatsAccessor
    {
        public CloseVoteQueueStats GetOverallQueueStats()
        {
            var doc = CQ.CreateFromUrl("http://stackoverflow.com/review/close/stats");
            var statsTable = doc.Find("table.task-stat-table");
            var cells = statsTable.Find("td");

            var stats = new CloseVoteQueueStats();

            stats.NeedReview = int.Parse(cells
                .ElementAt(0)
                .FirstElementChild
                .InnerText, NumberStyles.AllowThousands);

            stats.ReviewsToday = int.Parse(cells
                .ElementAt(1)
                .FirstElementChild
                .InnerText, NumberStyles.AllowThousands);

            stats.ReviewsAllTime = int.Parse(cells
                .ElementAt(2)
                .FirstElementChild
                .InnerText, NumberStyles.AllowThousands);

            return stats;
        }
    }

    internal class CloseVoteQueueStats
    {
        public int NeedReview { get; set; }
        public int ReviewsToday { get; set; }
        public int ReviewsAllTime { get; set; }
    }
}
