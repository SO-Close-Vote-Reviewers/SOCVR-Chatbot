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
        private InstallationSettings settings;

        public delegate void ShutdownOrderGivenHandler();
        public event ShutdownOrderGivenHandler ShutdownOrderGiven;

        /// <summary>
        /// Creates a new RoomManger object.
        /// </summary>
        public RoomManager()
        {

        }

        void cmp_StopBotCommandIssued()
        {
            LeaveRoom();

            if (ShutdownOrderGiven != null)
                ShutdownOrderGiven();
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

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            try
            {
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
    }
}
