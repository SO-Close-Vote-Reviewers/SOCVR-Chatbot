using System;

namespace SOCVR.Chatbot.Database
{
    /// <summary>
    /// Describes a review item that a user has completed.
    /// </summary>
    internal class UserReviewedItem
    {
        public int Id { get; set; }

        /// <summary>
        /// Null means the task was not an audit.
        /// True means the audit was passed, and false means it was failed.
        /// </summary>
        public bool? AuditPassed { get; set; }

        /// <summary>
        /// The date and time the user made the review.
        /// </summary>
        public DateTimeOffset ReviewedOn { get; set; }

        public virtual User Reviewer { get; set; }
        public int ReviewerId { get; set; }

        public ReviewItemAction ActionTaken { get; set; }

        public string PrimaryTag { get; set; }
    }

    // Do NOT alter enum order.
    internal enum ReviewItemAction
    {
        LeaveOpen,
        Close,
        Edit
    }
}
