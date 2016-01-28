using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SOCVR.Chatbot.ChatRoom
{
    /// <summary>
    /// Keeps track of which ChatbotActions are running.
    /// </summary>
    public static class RunningChatbotActionsManager
    {
        private static ConcurrentDictionary<Guid, RunningChatbotAction> runningChatbotActions
            = new ConcurrentDictionary<Guid, RunningChatbotAction>();

        /// <summary>
        /// Inserts the chatbot action into the list of currently running actions.
        /// Returns a guid of the process. Use this guid to mark the chatbot action as finished.
        /// </summary>
        /// <param name="chatbotActionName">The human-friendly name of the action.</param>
        /// <param name="runningForUserName">The user name of the person who started the action.</param>
        /// <param name="runningForUserId">The chat Id of the person who started the action.</param>
        /// <returns></returns>
        public static Guid MarkChatbotActionAsStarted(string chatbotActionName, string runningForUserName, int runningForUserId)
        {
            var processId = Guid.NewGuid();

            var newCommandEntry = new RunningChatbotAction()
            {
                ChatbotActionName = chatbotActionName,
                StartTs = DateTimeOffset.Now,
                RunningForUserId = runningForUserId,
                RunningForUserName = runningForUserName,
            };

            runningChatbotActions.TryAdd(processId, newCommandEntry);

            return processId;
        }

        /// <summary>
        /// Tells the manager that the process has completed and should be removed from the listing.
        /// </summary>
        /// <param name="processId">The process ID that you got from the MarkChatbotActionAsStarted method.</param>
        public static void MarkChatbotActionAsFinished(Guid processId)
        {
            RunningChatbotAction foundRunningCommand;
            if (!runningChatbotActions.TryRemove(processId, out foundRunningCommand))
            {
                throw new InvalidOperationException("Can't find a process with that Id");
            }
        }

        /// <summary>
        /// Returns a listing of all currently running chatbot actions.
        /// </summary>
        /// <returns></returns>
        public static List<RunningChatbotAction> GetRunningChatbotActions() => 
            runningChatbotActions.Select(x => x.Value).ToList();
    }

    /// <summary>
    /// Contains simple information about a running chatbot action.
    /// </summary>
    public class RunningChatbotAction
    {
        /// <summary>
        /// The human-friendly name of the chatbot action.
        /// </summary>
        public string ChatbotActionName { get; set; }

        /// <summary>
        /// The username of the chat user who started this action.
        /// </summary>
        public string RunningForUserName { get; set; }

        /// <summary>
        /// The chat Id of the user who started this action.
        /// </summary>
        public int RunningForUserId { get; set; }

        /// <summary>
        /// The date-time that the action was started.
        /// </summary>
        public DateTimeOffset StartTs { get; set; }
    }
}
