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
                var user = GetRegisteredUser(userMessage.AuthorID, db);
                CompletedAuditEntry newEntry = new CompletedAuditEntry()
                {
                    EntryTs = DateTimeOffset.Now,
                    RegisteredUser = user,
                    TagName = pattern.Match(userMessage.Content).Groups[1].Value
                };

                db.CompletedAuditEntries.Add(newEntry);
                db.SaveChanges();
            }

            //say the next tag on the list
        }

        /// <summary>
        /// Gets the user with the given ID, or creates the user and returns the newly created entry.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private RegisteredUser GetRegisteredUser(int userId, SOChatBotEntities db)
        {
            var existingUser = db.RegisteredUsers.SingleOrDefault(x => x.ChatProfileId == userId);

            if (existingUser != null)
                return existingUser;

            var newUser = new RegisteredUser() { ChatProfileId = userId };
            db.RegisteredUsers.Add(newUser);
            db.SaveChanges();

            return newUser;
        }
    }
}
