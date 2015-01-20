using ChatExchangeDotNet;
using CVChatbot.Bot.Model;
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
        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^passed (?:an )?(\S+) audit$";
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom)
        {
            var chatUser = chatRoom.GetUser(incommingChatMessage.AuthorID);
            Regex pattern = new Regex(RegexMatchingPattern);

            using (CVChatBotEntities db = new CVChatBotEntities())
            {
                var registedUser = db.RegisteredUsers
                    .Single(x => x.ChatProfileId == incommingChatMessage.AuthorID);

                CompletedAuditEntry newEntry = new CompletedAuditEntry()
                {
                    EntryTs = DateTimeOffset.Now,
                    RegisteredUser = registedUser,
                    TagName = pattern.Match(incommingChatMessage.Content).Groups[1].Value
                };

                db.CompletedAuditEntries.Add(newEntry);
                db.SaveChanges();
            }

            //say the next tag on the list
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
