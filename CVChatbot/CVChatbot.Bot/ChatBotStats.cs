using System;

namespace CVChatbot.Bot
{
    /// <summary>
    /// Class to hold status about how the chatbot is running for this instance.
    /// </summary>
    public static class ChatBotStats
    {
        /// <summary>
        /// The DateTime that the chatbot joined the chat room.
        /// </summary>
        public static DateTime LoginDate { get; set; }
    }
}
