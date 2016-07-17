using ChatExchangeDotNet;

namespace SOCVR.Chatbot.ChatbotActions
{
    /// <summary>
    /// Logic that the bot performs and directs output to the chatroom.
    /// </summary>
    internal abstract class ChatbotAction
    {
        ///// <summary>
        ///// Runs the action and returns the messages that should be posted to the chat room.
        ///// </summary>
        ///// <returns></returns>
        //public abstract List<string> RunAction();





        /// <summary>
        /// Runs the core logic to this action.
        /// </summary>
        /// <param name="chatRoom">The chat room the message was said in.</param>
        public abstract void RunAction(Room chatRoom);


    }
}
