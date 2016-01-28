//using System.Linq;
//using System.Text.RegularExpressions;

//namespace SOCVR.Chatbot.ChatbotActions.Triggers
//{
//    public class CompletedAudit : Trigger
//    {
//        public override string ActionDescription => null;

//        public override string ActionName => "Completed Audit";

//        public override string ActionUsage => null;

//        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

//        protected override string RegexMatchingPattern => @"^passed\s+(?:an?\s+)?(\S+)\s+audit$";



//        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
//        {
//            var chatUser = chatRoom.GetUser(incomingChatMessage.Author.ID);
//            var tagName = GetRegexMatchingObject()
//                    .Match(incomingChatMessage.Content)
//                    .Groups[1]
//                    .Value;

//            using (var db = new Database.DatabaseContext())
//            {
//                db.ReviewedItems.Add(new Database.UserReviewedItem
//                {
//                    A
//                });
//            }
//                var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
//            da.InsertCompletedAuditEntry(incomingChatMessage.Author.ID, tagName);
//        }
//    }
//}
