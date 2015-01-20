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
        public override void RunAction(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (CVChatBotEntities db = new CVChatBotEntities())
            {
                //get the last complete session

                var lastSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .Where(x => x.SessionEnd != null)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                if (lastSession == null)
                {
                    chatRoom.PostReply(userMessage, "You have no completed review sessions on record, so I can't give you any stats.");
                    return;
                }

                var sessionEndedTimeAgo = (DateTimeOffset.Now - lastSession.SessionEnd.Value);
                var sessionLength = lastSession.SessionEnd.Value - lastSession.SessionStart;

                string statMessage = "Your last completed review session ended {0} ago and lasted {1}. ";

                if (lastSession.ItemsReviewed == null)
                {
                    statMessage += "However, the number of reviewed items has not been set. Use the command `last session edit count <new count>` to set the new value.";
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

                //check if there is a on-going review session

                var ongoingSessions = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .Where(x => x.SessionEnd == null)
                    .Where(x => x.SessionStart > lastSession.SessionStart)
                    .OrderByDescending(x=>x.SessionStart)
                    .FirstOrDefault();

                if (ongoingSessions != null)
                {
                    var deltaTime = DateTimeOffset.Now - ongoingSessions.SessionStart;
                    statMessage += " **Note: You still have a review session in progress.** It started {0} ago.".FormatInline(deltaTime.ToUserFriendlyString());
                }

                chatRoom.PostReply(userMessage, statMessage);
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

        public override string GetCommandName()
        {
            return "Lass Session Stats";
        }

        public override string GetCommandDescription()
        {
            return "shows stats about your last review session";
        }

        public override string GetCommandUsage()
        {
            return "last session stats";
        }
    }
}
