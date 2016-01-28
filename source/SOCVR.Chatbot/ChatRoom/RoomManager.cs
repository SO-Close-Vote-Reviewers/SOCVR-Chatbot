using System;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using TCL.Extensions;
using SOCVR.Chatbot.Configuration;

namespace SOCVR.Chatbot.ChatRoom
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
        public delegate void InformationMessageBrodcastedHandler(string message, string author);

        public event ShutdownOrderGivenHandler ShutdownOrderGiven;
        public event InformationMessageBrodcastedHandler InformationMessageBroadcasted;

        /// <summary>
        /// Creates a new RoomManger object.
        /// Initializes the ChatMessageProcessor for internal use.
        /// </summary>
        public RoomManager() { }

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

        public void Dispose() => Dispose(!disposed);

        void cmp_StopBotCommandIssued(object sender, EventArgs e)
        {
            LeaveRoom();

            if (ShutdownOrderGiven != null)
                ShutdownOrderGiven(this, e);
        }

        /// <summary>
        /// Joins the room with the settings passed in.
        /// </summary>
        public void JoinRoom()
        {
            // Create the ChatMessageProcessor.
            cmp = new ChatMessageProcessor();
            cmp.StopBotCommandIssued += cmp_StopBotCommandIssued;

            // Logic to join the chat room.
            chatClient = new Client(ConfigurationAccessor.LoginEmail, ConfigurationAccessor.LoginPassword);
            cvChatRoom = chatClient.JoinRoom(ConfigurationAccessor.ChatRoomUrl);
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMention = false;

            // Say the startup message?
            if (!ConfigurationAccessor.StartUpMessage.IsNullOrWhiteSpace())
            {
                // This is the one of the few instances to not using the "OrThrow" method.
                var startMessage = cvChatRoom.PostMessage(ConfigurationAccessor.StartUpMessage);

                if (startMessage == null)
                {
                    throw new InvalidOperationException("Unable to post start up message to room.");
                }
            }

            cvChatRoom.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(cvChatRoom_NewMessage));
            cvChatRoom.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message>(cvChatRoom_NewMessage));
        }

        public void LeaveRoom()
        {
            // If there is a stop message, say it.
            if (!ConfigurationAccessor.StopMessage.IsNullOrWhiteSpace())
                cvChatRoom.PostMessage(ConfigurationAccessor.StopMessage);

            cvChatRoom.Leave();
        }

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            try
            {
                if (InformationMessageBroadcasted != null)
                    InformationMessageBroadcasted(newMessage.Content, newMessage.Author.Name);

                await Task.Run(() => cmp.ProcessChatMessage(newMessage, cvChatRoom));
            }
            catch (Exception ex)
            {
                // Something happened outside of an action's RunAction method. attempt to tell chat about it
                // this line will throw an exception if it fails, moving it further up the line.
                cvChatRoom.PostMessageOrThrow("error happened!\n" + ex.FullErrorMessage(Environment.NewLine)); // For now, more verbose later.
            }
        }
    }
}
