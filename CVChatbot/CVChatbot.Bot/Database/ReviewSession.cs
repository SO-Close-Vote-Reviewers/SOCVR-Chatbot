using System;

namespace SOCVR.Chatbot.Core.Database
{
    class ReviewSession
    {
        public int Id { get; set; }
        public int RegisteredUserId { get; set; }
        public DateTimeOffset SessionStart { get; set; }
        public DateTimeOffset? SessionEnd { get; set; }
        public int? ItemsReviewed { get; set; }
    }
}
