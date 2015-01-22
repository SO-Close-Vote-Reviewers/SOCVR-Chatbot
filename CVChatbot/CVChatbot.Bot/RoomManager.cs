using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot
{
    /// <summary>
    /// This class joins and keeps track of the chat room.
    /// </summary>
    public class RoomManager
    {
        private Room cvChatRoom;
        private Client chatClient;
        private ChatMessageProcessor cmp;
        private RoomManagerSettings settings;

        public RoomManager()
        {
            cmp = new ChatMessageProcessor();
        }

        /// <summary>
        /// Joins the room with the settings passed in from the constructor.
        /// </summary>
        public void JoinRoom(RoomManagerSettings managerSettings)
        {
            settings = managerSettings;

            chatClient = new Client(settings.Username, settings.Email, settings.Password);
            cvChatRoom = chatClient.JoinRoom(settings.ChatRoomUrl);
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMentionFromMessages = false;

            if (!settings.StartUpMessage.IsNullOrWhiteSpace())
            {
                //this is the one exception to not using the "OrThrow" method
                var startMessage = cvChatRoom.PostMessage(settings.StartUpMessage);

                if (startMessage == null)
                {
                    throw new InvalidOperationException("Unable to post start up message to room");
                }
            }

            cvChatRoom.NewMessage += cvChatRoom_NewMessage;
        }

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            try
            {
                await Task.Run(() => cmp.ProcessChatMessage(newMessage, cvChatRoom));
            }
            catch (Exception ex)
            {
                //something happened outside of an action's RunAction method. attempt to tell chat about it
                //this line will throw an exception if it fails, moving it futher up the line
                cvChatRoom.PostMessageOrThrow("error happened!\n" + ex.FullErrorMessage(Environment.NewLine)); //for now, more verbose later
            }
        }
    }

    public class RoomManagerSettings
    {
        public string ChatRoomUrl { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string StartUpMessage { get; set; }
    }
}
