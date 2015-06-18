/*
 * ChatExchange.Net. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using CsQuery;
using System;
using System.Linq;
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
