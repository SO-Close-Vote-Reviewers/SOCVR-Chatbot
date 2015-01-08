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
            cvChatRoom.StripMentionFromMessages = false;

            var startMessage = cvChatRoom.PostMessage("Hey everyone! (SO Close Vote Chatbot started)");

            if (startMessage == null)
            {
                throw new InvalidOperationException("Unable to post message to room");
            }

            cvChatRoom.NewMessage += cvChatRoom_NewMessage;
        }

        //private async Task ProcessChatMessage(Message newMessage)
        //{
        //    //first, check if the incoming message is addressed to the chat user

        //    if (newMessage.ParentID <= 0) //this is not a reply
        //        return;

        //    var repliedMessage = cvChatRoom.GetMessage(newMessage.ParentID);

        //    if (repliedMessage.AuthorID != cvChatRoom.Me.ID)
        //        return;

        //    //the message is directed to the chat user, process it
        //    //figure out if the message is a command


        //    if (newMessage.Content == "repeat")
        //    {
        //        await Task.Run(() =>
        //        {
        //            var messageId = newMessage.ParentID;

        //            if (messageId == -1)
        //            {
        //                cvChatRoom.PostReply(newMessage, "nothing to repeat");
        //            }
        //            else
        //            {
        //                var repeatMessage = cvChatRoom.GetMessage(newMessage.ParentID);
        //                cvChatRoom.PostReply(newMessage, repeatMessage.Content);
        //            }
        //        });
        //    }
        //}

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            await cmp.ProcessChatMessageAsync(newMessage, cvChatRoom);
        }
    }
}
