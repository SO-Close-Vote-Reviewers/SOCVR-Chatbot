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

        public async Task ProcessUserInputAsync(/* Message userMessage */)
        {
            // ok, ignore everything under this, i'm moving a lot of the logic to other locations

            /*
             * message is one of the following:
             * - user command (intentionally control the chat bot)
             * - trigger (user said something that the chat bot wants to react on)
             * - regular message (ignore)
             */



            //get a list of commands that could work

            //if there is more than one, throw error
            //if there is none, do nothing

            //get the single command to run
            UserCommand commandToRun;

            //await commandToRun.RunCommandAsync();
        }
    }
}
