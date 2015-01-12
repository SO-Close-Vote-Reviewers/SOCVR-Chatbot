using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using CVChatbot.Model;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Shows stats about the last session
    /// </summary>
    public class LastSessionStats : UserCommand
    {
        string matchPatternText = @"^last session stats$";

        public override bool DoesInputTriggerCommand(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);

            var message = userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim();

            return matchPattern.IsMatch(message);
        }

        public override void RunCommand(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                //get the last complete session

                var lastSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .Where(x => x.SessionEnd != null)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                if (lastSession == null)
                {
                    chatRoom.PostReply(userMessage, "You have no review sessions on record, so I can't give you any stats.");
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

                    statMessage += "You reviewed {2} items, averaging 1 review every {3}.";
                    statMessage = statMessage.FormatSafely(
                        sessionEndedTimeAgo.ToUserFriendlyString(), 
                        sessionLength.ToUserFriendlyString(),
                        lastSession.ItemsReviewed.Value,
                        averageTimePerReview.ToUserFriendlyString());
                }

                chatRoom.PostReply(userMessage, statMessage);
            }
        }
    }
}
