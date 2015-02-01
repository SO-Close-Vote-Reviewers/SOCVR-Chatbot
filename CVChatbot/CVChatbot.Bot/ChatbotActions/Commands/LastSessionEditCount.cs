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
    /// <summary>
    /// Used for editing the last completed session's review count.
    /// </summary>
    public class LastSessionEditCount : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            using (var db = new CVChatBotEntities())
            {
                // Get the last complete session.
                var lastSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == incommingChatMessage.AuthorID && x.SessionEnd != null)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                if (lastSession == null)
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "You have no completed review sessions on record, so I can't edit any entries.");
                    return;
                }

                var newReviewCount = GetRegexMatchingObject()
                    .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                    .Groups[1]
                    .Value
                    .Parse<int>();

                var previousReviewCount = lastSession.ItemsReviewed;
                lastSession.ItemsReviewed = newReviewCount;

                var replyMessage = @"    Review item count has been changed:
    User: {0} ({1})
    Start Time: {2}
    End Time: {3}
    Items Reviewed: {4} -> {5}
    Use the command 'last session stats' to see more details."
                    .FormatInline(
                        incommingChatMessage.AuthorName,
                        incommingChatMessage.AuthorID,
                        lastSession.SessionStart.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                        lastSession.SessionEnd.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                        previousReviewCount.HasValue
                            ? previousReviewCount.Value.ToString()
                            : "[Not Set]", 
                        lastSession.ItemsReviewed.Value);

                db.SaveChanges();
                chatRoom.PostReplyOrThrow(incommingChatMessage, replyMessage);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^last session edit count (\d+)$";
        }

        public override string GetActionName()
        {
            return "Last Session Edit Count";
        }

        public override string GetActionDescription()
        {
            return "Edits the number of reviewed items in your last review session.";
        }

        public override string GetActionUsage()
        {
            return "last session edit count <new count>";
        }
    }
}
