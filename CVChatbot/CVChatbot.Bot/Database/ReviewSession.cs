using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.Database
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
