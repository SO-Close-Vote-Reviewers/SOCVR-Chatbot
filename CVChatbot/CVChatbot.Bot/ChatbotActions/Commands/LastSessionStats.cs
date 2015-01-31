using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using CVChatbot.Bot.Model;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Shows stats about the last session
    /// </summary>
    public class LastSessionStats : UserCommand
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
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "You have no completed review sessions on record, so I can't give you any stats.");
                    return;
                }

                var sessionEndedTimeAgo = (DateTimeOffset.Now - lastSession.SessionEnd.Value);
                var sessionLength = lastSession.SessionEnd.Value - lastSession.SessionStart;
                var statMessage = "Your last completed review session ended {0} ago and lasted {1}. ";

                if (lastSession.ItemsReviewed == null)
                {
                    statMessage += "However, the number of reviewed items has not been set. Use the command `{0}` to set the new value."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<LastSessionEditCount>());
                    statMessage = statMessage.FormatSafely(
                        sessionEndedTimeAgo.ToUserFriendlyString(), 
                        sessionLength.ToUserFriendlyString());
                }
                else
                {
                    var averageTimePerReview = new TimeSpan(sessionLength.Ticks / (lastSession.ItemsReviewed.Value));

                    statMessage += "You reviewed {2} items, averaging a review every {3}.";
                    statMessage = statMessage.FormatSafely(
                        sessionEndedTimeAgo.ToUserFriendlyString(), 
                        sessionLength.ToUserFriendlyString(),
                        lastSession.ItemsReviewed.Value,
                        averageTimePerReview.ToUserFriendlyString());
                }

                // Check if there is a on-going review session.

                var ongoingSessions = db.ReviewSessions
                    .Where(x =>
                        x.RegisteredUser.ChatProfileId == incommingChatMessage.AuthorID && 
                        x.SessionEnd == null && 
                        x.SessionStart > lastSession.SessionStart)
                    .OrderByDescending(x=>x.SessionStart)
                    .FirstOrDefault();

                if (ongoingSessions != null)
                {
                    var deltaTime = DateTimeOffset.Now - ongoingSessions.SessionStart;
                    statMessage += " **Note: You still have a review session in progress.** It started {0} ago.".FormatInline(deltaTime.ToUserFriendlyString());
                }

                chatRoom.PostReplyOrThrow(incommingChatMessage, statMessage);
            }
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^last session stats$";
        }

        public override string GetActionName()
        {
            return "Lass Session Stats";
        }

        public override string GetActionDescription()
        {
            return "Shows stats about your last review session.";
        }

        public override string GetActionUsage()
        {
            return "last session stats";
        }
    }
}
