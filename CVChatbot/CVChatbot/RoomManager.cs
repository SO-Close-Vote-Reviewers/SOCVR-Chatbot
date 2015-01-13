using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

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

        public RoomManager()
        {
            cmp = new ChatMessageProcessor();

            var chatRoomUrl = SettingsAccessor.GetSettingValue<string>("ChatRoomUrl");
            var username = SettingsAccessor.GetSettingValue<string>("LoginUsername");
            var email = SettingsAccessor.GetSettingValue<string>("LoginEmail");
            var password = SettingsAccessor.GetSettingValue<string>("LoginPassword");

            chatClient = new Client(username, email, password);
            cvChatRoom = chatClient.JoinRoom(chatRoomUrl);
            ChatBotStats.LoginDate = DateTime.Now;
            cvChatRoom.StripMentionFromMessages = false;

            var startUpMessage = SettingsAccessor.GetSettingValue<string>("StartUpMessage");

            if (!startUpMessage.IsNullOrWhiteSpace())
            {
                var startMessage = cvChatRoom.PostMessage(startUpMessage);

                if (startMessage == null)
                {
                    throw new InvalidOperationException("Unable to post message to room");
                }
            }

            cvChatRoom.NewMessage += cvChatRoom_NewMessage;
        }

        private async void cvChatRoom_NewMessage(Message newMessage)
        {
            await cmp.ProcessChatMessageAsync(newMessage, cvChatRoom);
        }
    }
}
