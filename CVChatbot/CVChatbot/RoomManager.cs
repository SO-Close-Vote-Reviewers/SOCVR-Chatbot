using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    /// <summary>
    /// This class handles all interactions to and from the chatroom.
    /// </summary>
    public class RoomManager
    {
        private Room cvChatRoom;
        private Client chatClient;

        public RoomManager(string username, string email, string password)
        {
            chatClient = new Client(username, email, password);
            cvChatRoom = chatClient.JoinRoom("http://chat.stackoverflow.com/rooms/1/sandbox");

            var startMessage = cvChatRoom.PostMessage("Hey everyone! (SO Close Vote Chatbot started)");

            if (startMessage == null)
            {
                throw new InvalidOperationException("Unable to post message to room");
            }

            cvChatRoom.NewMessage += cvChatRoom_NewMessage;
        }

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            if (newMessage.Content == "repeat")
            {
                await Task.Run(() =>
                {
                    var messageId = newMessage.ParentID;

                    if (messageId == -1)
                    {
                        cvChatRoom.PostReply(newMessage, "nothing to repeat");
                    }
                    else
                    {
                        var repeatMessage = cvChatRoom.GetMessage(newMessage.ParentID);
                        cvChatRoom.PostReply(newMessage, repeatMessage.Content);
                    }
                });
            }
        }


    }
}
