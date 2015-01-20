using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    public static class RunningCommandsManager
    {
        private static ConcurrentDictionary<Guid, RunningCommand> runningCommands
            = new ConcurrentDictionary<Guid, RunningCommand>();

        /// <summary>
        /// Inserts the command into the list of currently running commands.
        /// Returns a guid of the process. Use this guid to mark the command as finished.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="runningForUserName"></param>
        /// <param name="runningForUserId"></param>
        /// <returns></returns>
        public static Guid MarkCommandAsStarted(string commandName, string runningForUserName, int runningForUserId)
        {
            var processId = Guid.NewGuid();

            var newCommandEntry = new RunningCommand()
            {
                CommandName = commandName,
                CommandStartTs = DateTimeOffset.Now,
                RunningForUserId = runningForUserId,
                RunningForUserName = runningForUserName,
            };

            runningCommands.TryAdd(processId, newCommandEntry);

            return processId;
        }

        public static void MarkCommandAsFinished(Guid processId)
        {
            RunningCommand foundRunningCommand;
            if (!runningCommands.TryRemove(processId, out foundRunningCommand))
            {
                throw new InvalidOperationException("Can't find a process with that Id");
            }
        }

        public static List<RunningCommand> GetRunningCommands()
        {
            return runningCommands
                .Select(x => x.Value)
                .ToList();
        }
    }
}
