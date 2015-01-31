using CVChatbot.Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class TrackUser : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var userIdToAdd = GetRegexMatchingObject()
                .Match(GetMessageContentsReadyForRegexParsing(userMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            using (var db = new CVChatBotEntities())
            {
                var existingUser = db.RegisteredUsers
                    .SingleOrDefault(x => x.ChatProfileId == userIdToAdd);

                if (existingUser != null)
                {
                    chatRoom.PostReplyOrThrow(userMessage, "That user is already in the system!");
                    return;
                }

                var newUser = new RegisteredUser()
                {
                    ChatProfileId = userIdToAdd
                };

                db.RegisteredUsers.Add(newUser);
                db.SaveChanges();

                var chatUser = chatRoom.GetUser(userIdToAdd);

                chatRoom.PostReplyOrThrow(userMessage, "Ok, I added {0} ({1}) to the tracked users list."
                    .FormatInline(chatUser.Name, chatUser.ID));
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Owner;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:add|track) user (\d+)$";
        }

        public override string GetActionName()
        {
            return "Add user";
        }

        public override string GetActionDescription()
        {
            return "Adds the user to the registered users list.";
        }

        public override string GetActionUsage()
        {
            return "(add | track) user <chat id>";
        }
    }
}
