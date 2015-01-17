using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    public abstract class ChatbotAction
    {
        public abstract ActionPermissionLevel GetPermissionLevel();

        public abstract bool DoesChatMessageActivateAction(Message chatMessage);

        public abstract void RunAction(Message chatMessage, Room chatRoom);

        public abstract ActionType Type { get; set; }
    }

    public enum ActionType
    {
        /// <summary>
        /// This is a message addressed to the chat bot, and the user wants to the chat bot to do something.
        /// </summary>
        Command,

        /// <summary>
        /// The user has said something that the chat bot will react on. This message is not directed at the chat bot.
        /// </summary>
        Trigger
    }

    public enum ActionPermissionLevel
    {
        /// <summary>
        /// All people who join the chat room are allowed to run this command or activate the trigger
        /// </summary>
        Everyone,

        /// <summary>
        /// Only people in the tracked users list can run this command or activate the trigger
        /// </summary>
        Registered,

        /// <summary>
        /// Only people in the tracked users list who are labeled as "owner" can run this command or activate the trigger
        /// </summary>
        Owner,
    }
}
