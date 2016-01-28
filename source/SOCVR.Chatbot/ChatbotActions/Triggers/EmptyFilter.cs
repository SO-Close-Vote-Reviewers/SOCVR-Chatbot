//using System.Linq;
//using System.Text.RegularExpressions;

//namespace SOCVR.Chatbot.ChatbotActions.Triggers
//{
//    public class EmptyFilter : Trigger
//    {
//        private Regex tagMatchingPattern = new Regex(@"\[(\S+?)\] ?", RegexObjOptions);

//        public override string ActionDescription => null;

//        public override string ActionName => "Empty Filter";

//        public override string ActionUsage => null;

//        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

//        protected override string RegexMatchingPattern => "^(?:> +)?there are no items for you to review, matching the filter \"(?:[A-z-, ']+; )?([\\S ]+)\"";



//        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
//        {
//            // First, get the tags that were used.
//            string tags = GetRegexMatchingObject()
//                    .Match(incomingChatMessage.Content)
//                    .Groups[1]
//                    .Value;

//            // Split out tags.
//            var parsedTagNames = tagMatchingPattern.Matches(incomingChatMessage.Content.ToLower())
//                .Cast<Match>()
//                .Select(x => x.Groups[1].Value)
//                .ToList();

//            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);

//            // Save the tags to the database.
//            foreach (var tagName in parsedTagNames)
//            {
//                da.InsertNoItemsInFilterRecord(incomingChatMessage.Author.ID, tagName);
//            }
//        }
//    }
//}
