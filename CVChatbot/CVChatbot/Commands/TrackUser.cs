using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using CVChatbot.Model;

namespace CVChatbot.Commands
{
    public class TrackUser : UserCommand
    {
        string matchPatternText = @"^(?:add|track) user (\d+)$";

        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);

            var message = userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();

            return matchPattern.IsMatch(message);
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            Regex matchPattern = new Regex(matchPatternText);

            var message = userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();

            var userIdToAdd = matchPattern.Match(message).Groups[1].Value.Parse<int>();

            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                var existingUser = db.RegisteredUsers
                    .SingleOrDefault(x => x.ChatProfileId == userIdToAdd);

                if (existingUser != null)
                {
                    chatRoom.PostReply(userMessage, "That user is already in the system!");
                    return;
                }

                var newUser = new RegisteredUser()
                {
                    ChatProfileId = userIdToAdd
                };

                db.RegisteredUsers.Add(newUser);
                db.SaveChanges();

                var chatUser = chatRoom.GetUser(userIdToAdd);

                chatRoom.PostReply(userMessage, "Ok, I added {0} ({1}) to the tracked users list."
                    .FormatInline(chatUser.Name, chatUser.ID));
            }
        }
    }
}
