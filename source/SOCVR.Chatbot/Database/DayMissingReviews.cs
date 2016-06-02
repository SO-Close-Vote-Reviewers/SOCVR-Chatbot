using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCVR.Chatbot.Database
{
    class DayMissingReviews
    {
        /// <summary>
        /// The User that this entry is for.
        /// </summary>
        public User User { get; set; }

        public int ProfileId { get; set; }

        /// <summary>
        /// The UTC Date for this record.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of reviews that are unaccounted for. 
        /// This could be because the review is an audit about a deleted post, and thus can't be parsed.
        /// </summary>
        public int MissingReviewsCount { get; set; }
    }
}
