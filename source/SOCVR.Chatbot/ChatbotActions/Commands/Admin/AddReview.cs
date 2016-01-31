using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Admin
{
    internal class AddReview : UserCommand
    {
        public override string ActionDescription => "Manually adds a review to a user. Should only be used for testing.";

        public override string ActionName => "Add Review";

        public override string ActionUsage => "add review [review id] [user id]";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        protected override string RegexMatchingPattern => @"^add review (\d+) (\d+)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var reviewItemId = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[1]
                    .Value
                    .Parse<int>();

                var reviewerId = GetRegexMatchingObject()
                    .Match(incomingChatMessage.Content)
                    .Groups[2]
                    .Value
                    .Parse<int>();

                //look up reviewer
#warning make a method to "get user or new"
                var targetUser = db.Users.SingleOrDefault(x => x.ProfileId == incomingChatMessage.Author.ID);

                var newReview = null; //this will use the method from the user tracking
                //http://chat.stackoverflow.com/transcript/message/28474023#28474023

                db.ReviewedItems.Add(newReview);
                db.SaveChanges();
            }
        }
    }
}
