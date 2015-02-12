using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    public class CompletedAudit : Trigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var chatUser = chatRoom.GetUser(incommingChatMessage.AuthorID);
            var tagName = GetRegexMatchingObject()
                    .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                    .Groups[1]
                    .Value;

            using (var db = new CVChatBotEntities())
            {
                var registedUser = db.RegisteredUsers
                    .Single(x => x.ChatProfileId == incommingChatMessage.AuthorID);

                var newEntry = new CompletedAuditEntry()
                {
                    EntryTs = DateTimeOffset.Now,
                    RegisteredUser = registedUser,
                    TagName = tagName
                };

                db.CompletedAuditEntries.Add(newEntry);
                db.SaveChanges();
            }

            // Say the next tag on the list.
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^passed\s+(?:an?\s+)?(\S+)\s+audit$";
        }

        public override string GetActionName()
        {
            return "Completed Audit";
        }

        public override string GetActionDescription()
        {
            return null;
        }

        public override string GetActionUsage()
        {
            return null;
        }
    }
}
