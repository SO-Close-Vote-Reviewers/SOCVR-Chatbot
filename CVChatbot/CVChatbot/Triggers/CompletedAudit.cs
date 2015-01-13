using ChatExchangeDotNet;
using CVChatbot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Triggers
{
    public class CompletedAudit : Trigger
    {
        private const string RegexMatchingPattern = @"^passed (?:an )?(\S+) audit$";

        public override bool DoesInputActivateTrigger(Message userMessage)
        {
            Regex pattern = new Regex(RegexMatchingPattern);

            return pattern.IsMatch(userMessage.Content);
        }

        public override void RunTrigger(Message userMessage, Room chatRoom)
        {
            var chatUser = chatRoom.GetUser(userMessage.AuthorID);
            Regex pattern = new Regex(RegexMatchingPattern);

            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                var registedUser = db.RegisteredUsers
                    .Single(x => x.ChatProfileId == userMessage.AuthorID);

                CompletedAuditEntry newEntry = new CompletedAuditEntry()
                {
                    EntryTs = DateTimeOffset.Now,
                    RegisteredUser = registedUser,
                    TagName = pattern.Match(userMessage.Content).Groups[1].Value
                };

                db.CompletedAuditEntries.Add(newEntry);
                db.SaveChanges();
            }

            //say the next tag on the list
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }
    }
}
