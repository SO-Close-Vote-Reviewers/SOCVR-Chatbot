using System.Text.RegularExpressions;
using SOCVR.Chatbot.Bot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Used for editing the last completed session's review count.
    /// </summary>
    public class LastSessionEditCount : UserCommand
    {
        private Regex ptn = new Regex(@"^last session edit count (\d+)$", RegexObjOptions);

        public override string ActionDescription =>
            "Edits the number of reviewed items in your last review session.";

        public override string ActionName => "Last Session Edit Count";

        public override string ActionUsage => "last session edit count <new count>";

        public override ActionPermissionLevel PermissionLevel => ActionPermissionLevel.Registered;

        protected override Regex RegexMatchingObject => ptn;



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            var lastSession = da.GetLatestCompletedSession(incommingChatMessage.Author.ID);

            if (lastSession == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "You have no completed review sessions on record, so I can't edit any entries.");
                return;
            }

            var newReviewCount = RegexMatchingObject
                .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                .Groups[1]
                .Value
                .Parse<int>();

            if (newReviewCount < 0)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "New review count cannot be negative.");
                return;
            }

            var previousReviewCount = lastSession.ItemsReviewed;
            lastSession.ItemsReviewed = newReviewCount;

            var replyMessage = @"    Review item count has been changed:
    User: {0} ({1})
    Start Time: {2}
    End Time: {3}
    Items Reviewed: {4} -> {5}
    Use the command 'last session stats' to see more details."
                .FormatInline(
                    incommingChatMessage.Author.Name,
                    incommingChatMessage.Author.ID,
                    lastSession.SessionStart.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                    lastSession.SessionEnd.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                    previousReviewCount.HasValue
                        ? previousReviewCount.Value.ToString()
                        : "[Not Set]",
                    lastSession.ItemsReviewed.Value);

            da.EditLatestCompletedSessionItemsReviewedCount(lastSession.Id, newReviewCount);

            chatRoom.PostReplyOrThrow(incommingChatMessage, replyMessage);
        }
    }
}
