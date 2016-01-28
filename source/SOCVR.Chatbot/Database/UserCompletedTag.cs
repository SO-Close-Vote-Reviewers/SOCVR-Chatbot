using System;

namespace SOCVR.Chatbot.Bot.Database
{
    public class UserCompletedTag
    {
        public string TagName { get; set; }
        public int TimesCleared { get; set; }
        public DateTimeOffset LastTimeCleared { get; set; }
    }
}
