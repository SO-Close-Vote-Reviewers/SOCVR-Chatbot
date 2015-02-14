using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.Database
{
    class CompletedTag
    {
        public string TagName { get; set; }
        public int PeopleWhoCompletedTag { get; set; }
        public DateTimeOffset LastEntryTs { get; set; }
    }
}
