using ChatExchangeDotNet;
using CVChatbot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    public class UserCommandProcessor
    {
        private List<UserCommand> userCommands;

        public UserCommandProcessor()
        {
            userCommands = new List<UserCommand>();
        }

        public void AddUserCommand<TUserCommand>()
            where TUserCommand : UserCommand, new()
        {
            userCommands.Add(new TUserCommand());
        }

        /// <summary>
        /// Determines which user command to run based on the chat message.
        /// Message must be addressed to the chat bot.
        /// Returns null if no command could be found.
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="chatRoom"></param>
        /// <returns></returns>
        public UserCommand GetUserCommand(Message chatMessage, Room chatRoom)
        {
            var possibleUserCommands = userCommands
                .Where(x => x.DoesInputTriggerCommand(chatMessage))
                .ToList();

            if (possibleUserCommands.Count > 1)
                throw new Exception(string.Format("Found more than one user command for the input '{0}'",
                    chatMessage.Content));

            return possibleUserCommands.SingleOrDefault();
        }

        //public async Task ProcessUserInputAsync(/* Message userMessage */)
        //{
        //    // ok, ignore everything under this, i'm moving a lot of the logic to other locations

        //    /*
        //     * message is one of the following:
        //     * - user command (intentionally control the chat bot)
        //     * - trigger (user said something that the chat bot wants to react on)
        //     * - regular message (ignore)
        //     */



        //    //get a list of commands that could work

        //    //if there is more than one, throw error
        //    //if there is none, do nothing

        //    //get the single command to run
        //    UserCommand commandToRun;

        //    //await commandToRun.RunCommandAsync();
        //}
    }
}
