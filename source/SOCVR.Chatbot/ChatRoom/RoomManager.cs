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
    public class RoomManager : IDisposable
    {
        private Room cvChatRoom;
        private Client chatClient;
        private ChatMessageProcessor cmp;
        private bool disposed = false;

        public delegate void ShutdownOrderGivenHandler(object sender, EventArgs e);
        public delegate void InformationMessageBrodcastedHandler(string message, string author);

        public event ShutdownOrderGivenHandler ShutdownOrderGiven;
        public event InformationMessageBrodcastedHandler InformationMessageBroadcasted;

        public Room CvChatRoom => cvChatRoom;

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

            ShutdownOrderGiven?.Invoke(this, e);
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
            cvChatRoom = chatClient.JoinRoom(ConfigurationAccessor.ChatRoomUrl, true);
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMention = true;
            cvChatRoom.InitialisePrimaryContentOnly = true;
        }

        public void ConnectEventDelegates()
        {
            cvChatRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(cvChatRoom_NewPing));
            cvChatRoom.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(cvChatRoom_NewMessage));
        }

        public void PostStartupMessage()
        {
            // Say the startup message?
            if (!ConfigurationAccessor.StartUpMessage.IsNullOrWhiteSpace())
            {
                // This is the one of the few instances to not using the "OrThrow" method.
                var startupMessageText = $"{ConfigurationAccessor.StartUpMessage} ({ConfigurationAccessor.InstallationLocation})";
                var success = cvChatRoom.PostMessageLight(startupMessageText);

                if (!success)
                {
                    throw new InvalidOperationException("Unable to post start up message to room.");
                }
            }
        }

        public void LeaveRoom()
        {
            // If there is a stop message, say it.
            if (!ConfigurationAccessor.StopMessage.IsNullOrWhiteSpace())
            {
                var shutdownMessage = $"{ConfigurationAccessor.StopMessage} ({ConfigurationAccessor.InstallationLocation})";
                cvChatRoom.PostMessageLight(shutdownMessage);
            }

            cvChatRoom.Leave();
        }

        private async void cvChatRoom_NewPing(Message newMessage)
        {
            try
            {
                InformationMessageBroadcasted?.Invoke($"[ping] {newMessage.Content}", newMessage.Author.Name);

                await Task.Run(() => cmp.ProcessPing(newMessage, cvChatRoom));
            }
            catch (Exception ex)
            {
                // Something happened outside of an action's RunAction method. Attempt to tell chat about it
                // this line will throw an exception if it fails, moving it further up the line.
                cvChatRoom.PostMessageOrThrow($"Error happened!\n\n{ex}");
            }
        }

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            try
            {
                InformationMessageBroadcasted?.Invoke($"[message] {newMessage.Content}", newMessage.Author.Name);

                await Task.Run(() => cmp.ProcessNewMessage(newMessage, cvChatRoom));
            }
            catch (Exception ex)
            {
                // Something happened outside of an action's RunAction method. Attempt to tell chat about it
                // this line will throw an exception if it fails, moving it further up the line.
                cvChatRoom.PostMessageOrThrow($"Error happened!\n\n{ex}");
            }
        }
    }
}
