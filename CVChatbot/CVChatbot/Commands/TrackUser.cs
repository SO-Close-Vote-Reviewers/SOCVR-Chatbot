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
        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var userIdToAdd = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(userMessage))
                .Groups[1]
                .Value
                .Parse<int>();

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

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Owner;
        }

        protected override string GetMatchPattern()
        {
            return @"^(?:add|track) user (\d+)$";
        }

        public override string GetCommandName()
        {
            return "Add user";
        }

        public override string GetCommandDescription()
        {
            return "adds the user to the registered users list";
        }

        public override string GetCommandUsage()
        {
            return "(add | track) user <chat id>";
        }
    }
}
