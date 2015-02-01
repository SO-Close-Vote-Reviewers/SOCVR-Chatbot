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
    public class RoomManager:IDisposable
    {
        private Room cvChatRoom;
        private Client chatClient;
        private ChatMessageProcessor cmp;
        private bool disposed = false;

        public delegate void ShutdownOrderGivenHandler(object sender, EventArgs e);
        public event ShutdownOrderGivenHandler ShutdownOrderGiven;

        /// <summary>
        /// Creates a new RoomManger object.
        /// Initializes the ChatMessageProcessor for internal use.
        /// </summary>
        public RoomManager()
        {
            cmp = new ChatMessageProcessor();
            cmp.StopBotCommandIssued += cmp_StopBotCommandIssued;
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                if (chatClient != null)
                {
                    ((IDisposable)chatClient).Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(!disposed);
        }

        void cmp_StopBotCommandIssued(object sender, EventArgs e)
        {
            LeaveRoom("Goodbye!");

            if (ShutdownOrderGiven != null)
                ShutdownOrderGiven(this, e);
        }

        /// <summary>
        /// Joins the room with the settings passed in.
        /// </summary>
        public void JoinRoom(RoomManagerSettings managerSettings)
        {
            var settings = managerSettings;

            chatClient = new Client(settings.Email, settings.Password);
            cvChatRoom = chatClient.JoinRoom(settings.ChatRoomUrl);
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMentionFromMessages = false;

            if (!settings.StartUpMessage.IsNullOrWhiteSpace())
            {
                // This is the one exception to not using the "OrThrow" method.
                var startMessage = cvChatRoom.PostMessage(settings.StartUpMessage);

                if (startMessage == null)
                {
                    throw new InvalidOperationException("Unable to post start up message to room.");
                }
            }

            cvChatRoom.NewMessage += cvChatRoom_NewMessage;
        }

        public void LeaveRoom(string stopMessage)
        {
            cvChatRoom.PostMessage(stopMessage);
            cvChatRoom.Leave();
        }

        private async void cvChatRoom_NewMessage(object sender , MessageEventArgs e)
        {
            try
            {
                await Task.Run(() => cmp.ProcessChatMessage(e.Message, cvChatRoom));
            }
            catch (Exception ex)
            {
                // Something happened outside of an action's RunAction method. attempt to tell chat about it
                // this line will throw an exception if it fails, moving it further up the line.
                cvChatRoom.PostMessageOrThrow("error happened!\n" + ex.FullErrorMessage(Environment.NewLine)); // For now, more verbose later.
            }
        }
    }

    /// <summary>
    /// Settings needed to join a room.
    /// </summary>
    public class RoomManagerSettings
    {
        /// <summary>
        /// The url of the chat room to join.
        /// </summary>
        public string ChatRoomUrl { get; set; }

        /// <summary>
        /// The username of the account that is joining.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The Stack Exchange OAuth email to login with.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The Stack Exchange OAuth password to login with.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The message that the bot will announce when it first enters the chat room.
        /// If the message is null, empty, or entirely whitespace, then no announcement message will be said.
        /// </summary>
        public string StartUpMessage { get; set; }
    }
}
