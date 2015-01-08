using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    /// <summary>
    /// This class joins and keeps track of the chat room.
    /// </summary>
    public class RoomManager
    {
        private Room cvChatRoom;
        private Client chatClient;
        private ChatMessageProcessor cmp;

        public RoomManager(string username, string email, string password)
        {
            cmp = new ChatMessageProcessor();

            chatClient = new Client(username, email, password);
            cvChatRoom = chatClient.JoinRoom("http://chat.stackoverflow.com/rooms/68414/cvchatbot-testing");
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMentionFromMessages = false;

            var startMessage = cvChatRoom.PostMessage("Hey everyone! (SO Close Vote Chatbot started)");

            if (startMessage == null)
            {
                throw new InvalidOperationException("Unable to post message to room");
            }

            cvChatRoom.NewMessage += cvChatRoom_NewMessage;
        }

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            await cmp.ProcessChatMessageAsync(newMessage, cvChatRoom);
        }
    }
}
