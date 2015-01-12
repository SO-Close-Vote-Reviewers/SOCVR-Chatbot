using ChatExchangeDotNet;
using CVChatbot.Commands;
using CVChatbot.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot
{
    /// <summary>
    /// A class to take a chat message and acts on it if meets certain criteria
    /// </summary>
    public class ChatMessageProcessor
    {
        //private UserCommandProcessor ucp;
        private List<UserCommand> userCommands;
        private List<Trigger> triggers;

        public ChatMessageProcessor()
        {
            userCommands = new List<UserCommand>();
            triggers = new List<Trigger>();

            AddUserCommand<Alive>();
            AddUserCommand<Help>();
            AddUserCommand<Status>();
            AddUserCommand<Stats>();

            AddTrigger<CompletedAudit>();
            AddTrigger<EmptyFilter>();
        }

        private void AddUserCommand<TUserCommand>()
            where TUserCommand : UserCommand, new()
        {
            userCommands.Add(new TUserCommand());
        }

        private void AddTrigger<TTrigger>()
            where TTrigger : Trigger, new()
        {
            triggers.Add(new TTrigger());
        }

        public async Task ProcessChatMessageAsync(Message chatMessage, Room chatRoom)
        {
            await Task.Run(() => ProcessChatMessage(chatMessage, chatRoom));
        }

        private void ProcessChatMessage(Message chatMessage, Room chatRoom)
        {
            bool isReplyToChatbot = MessageIsReplyToChatbot(chatMessage, chatRoom);

            if (isReplyToChatbot)
            {
                //check if it's a command
                var userCommandToRun = GetUserCommand(chatMessage, chatRoom);
                if (userCommandToRun != null)
                {
                    userCommandToRun.RunCommand(chatMessage, chatRoom);
                }
                else
                {
                    chatRoom.PostReply(chatMessage, "Sorry, don't understand that.");
                }
            }
            else
            {
                //check if it's a trigger
                var triggerToRun = GetTrigger(chatMessage, chatRoom);
                if (triggerToRun != null)
                {
                    triggerToRun.RunTrigger(chatMessage, chatRoom);
                }
                
                //else, do nothing
            }
        }

        /// <summary>
        /// Determines which user command to run based on the chat message.
        /// Message must be addressed to the chat bot.
        /// Returns null if no command could be found.
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="chatRoom"></param>
        /// <returns></returns>
        private UserCommand GetUserCommand(Message chatMessage, Room chatRoom)
        {
            var possibleUserCommands = userCommands
                .Where(x => x.DoesInputTriggerCommand(chatMessage))
                .ToList();

            if (possibleUserCommands.Count > 1)
                throw new Exception("Found more than one user command for the input '{0}'"
                    .FormatSafely(chatMessage.Content));

            return possibleUserCommands.SingleOrDefault();
        }

        private Trigger GetTrigger(Message chatMessage, Room chatRoom)
        {
            var possibleTriggers = triggers
                .Where(x => x.DoesInputActivateTrigger(chatMessage))
                .ToList();

            if (possibleTriggers.Count > 1)
                throw new Exception("Found more than one trigger for the input '{0}'"
                    .FormatSafely(chatMessage.Content));

            return possibleTriggers.SingleOrDefault();
        }

        private bool MessageIsReplyToChatbot(Message chatMessage, Room chatRoom)
        {
            if (chatMessage.ParentID == -1)
                return false;

            var parentMessage = chatRoom.GetMessage(chatMessage.ParentID);

            return parentMessage.AuthorID == chatRoom.Me.ID;
        }
    }
}
