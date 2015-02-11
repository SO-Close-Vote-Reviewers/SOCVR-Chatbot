using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.Database
{
    class UserAuditStatEntry
    {
        public string TagName { get; set; }
        public decimal Percent { get; set; }
        public int Count { get; set; }
    }
}
