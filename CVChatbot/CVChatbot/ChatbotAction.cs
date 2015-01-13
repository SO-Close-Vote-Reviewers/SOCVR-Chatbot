using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    public abstract class ChatbotAction
    {
        public abstract void GetPermissionLevel();
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
