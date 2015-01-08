using ChatExchangeDotNet;
using CVChatbot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    /// <summary>
    /// A class to take a chat message and acts on it if meets certain criteria
    /// </summary>
    public class ChatMessageProcessor
    {
        private UserCommandProcessor ucp;

        public ChatMessageProcessor()
        {
            ucp = new UserCommandProcessor();
            ucp.AddUserCommand<Alive>();
            ucp.AddUserCommand<Help>();
            ucp.AddUserCommand<Status>();
            ucp.AddUserCommand<Stats>();
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
                var userCommandToRun = ucp.GetUserCommand(chatMessage, chatRoom);
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
            }
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
