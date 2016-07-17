using System;

namespace SOCVR.Chatbot.Database
{
#warning not going to lie, this is a pretty bad name. Open for suggestions.

    /// <summary>
    /// Holds information about the number of missing reviews for a given person on a given UTC day.
    /// Add this to the number of parsed reviews to obtain the true number of reviews a person did on a day.
    /// </summary>
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
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// The number of reviews that are unaccounted for. 
        /// This could be because the review is an audit about a deleted post, and thus can't be parsed.
        /// </summary>
        public int MissingReviewsCount { get; set; }
    }
}
