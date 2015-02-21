using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.Extensions;

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
        private InstallationSettings settings;
        private bool disposed = false;

        public delegate void ShutdownOrderGivenHandler(object sender, EventArgs e);
        public event ShutdownOrderGivenHandler ShutdownOrderGiven;

        /// <summary>
        /// Creates a new RoomManger object.
        /// Initializes the ChatMessageProcessor for internal use.
        /// </summary>
        public RoomManager()
        {

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
            LeaveRoom();

            if (ShutdownOrderGiven != null)
                ShutdownOrderGiven(this, e);
        }

        /// <summary>
        /// Joins the room with the settings passed in.
        /// </summary>
        public void JoinRoom(InstallationSettings settings)
        {
            // Copy over the settings into this class so this class can use it.
            this.settings = settings;

            // Create the ChatMessageProcessor.
            cmp = new ChatMessageProcessor(settings);
            cmp.StopBotCommandIssued += cmp_StopBotCommandIssued;

            // Logic to join the chat room.
            chatClient = new Client(settings.Email, settings.Password);
            cvChatRoom = chatClient.JoinRoom(settings.ChatRoomUrl);
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMentionFromMessages = false;

            // Say the startup message?
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

        public void LeaveRoom()
        {
            // If there is a stop message, say it.
            if (!settings.StopMessage.IsNullOrWhiteSpace())
                cvChatRoom.PostMessage(settings.StopMessage);

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
    public class InstallationSettings
    {
        /// <summary>
        /// The url of the chat room to join.
        /// </summary>
        public string ChatRoomUrl { get; set; }

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

        /// <summary>
        /// The message that the bot will announce when it shuts down.
        /// If the message is null, empty, or entirely whitespace, then no announcement message will be said.
        /// </summary>
        public string StopMessage { get; set; }

        /// <summary>
        /// The maximum number of hours a review session can last and still be closed by a trigger.
        /// If a session is longer than this value, only a forced EndSession command can end the session.
        /// </summary>
        public int MaxReviewLengthHours { get; set; }

        /// <summary>
        /// The default value used in the CompletedTags command when no argument is given.
        /// </summary>
        public int DefaultCompletedTagsPeopleThreshold { get; set; }

        /// <summary>
        /// The maximum number of tags that can be fetched with the NextTags command.
        /// </summary>
        public int MaxTagsToFetch { get; set; }

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// When using the Ping Reviewers command, this is the max numbers of days back to search for users by their review sessions.
        /// </summary>
        public int PingReviewersDaysBackThreshold { get; set; }

        /// <summary>
        /// This is the default number of tags to fetch if no argument is used in the Next Tags command.
        /// </summary>
        public int DefaultNextTagCount { get; set; }
    }
}
