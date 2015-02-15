using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.Database
{
    public class UserCompletedTag
    {
        public string TagName { get; set; }
        public int TimesCleared { get; set; }
        public DateTimeOffset LastTimeCleared { get; set; }
    }
}
