using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CVChatbot.Model;
using System.Configuration;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Triggers
{
    /// <summary>
    /// For when a person does 40 review items and has ran out of review actions
    /// </summary>
    public class OutOfReviewActions : Trigger
    {
        string matchPatternText = @"^(?:> )?Thank you for reviewing (\d+) close votes today; come back in ([\w ]+) to continue reviewing.$";

        public override bool DoesInputActivateTrigger(ChatExchangeDotNet.Message userMessage)
        {
            Regex matchPattern = new Regex(matchPatternText);
            return matchPattern.IsMatch(userMessage.Content);
        }

        public override void RunTrigger(ChatExchangeDotNet.Message userMessage, ChatExchangeDotNet.Room chatRoom)
        {
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                //find the latest session by that user
                var latestSession = db.ReviewSessions
                    .Where(x => x.RegisteredUser.ChatProfileId == userMessage.AuthorID)
                    .Where(x => x.SessionEnd == null)
                    .OrderByDescending(x => x.SessionStart)
                    .FirstOrDefault();

                //first, check if there is a session
                if (latestSession == null)
                {
                    chatRoom.PostReply(userMessage, "I don't seem to have the start of your review session on record. I might have not been running when you started, or some error happened.");
                    return;
                }

                //check if session is greater than [MAX_REVIEW_TIME]
                var maxReviewTimeHours = ConfigurationManager.AppSettings["MAX_REVIEW_LENGTH_HOURS"].Parse<int>();

                var timeThreshold = DateTimeOffset.Now.AddHours(-maxReviewTimeHours);

                if (latestSession.SessionStart < timeThreshold)
                {
                    chatRoom.PostReply(userMessage, "Your last uncompleted review session was greater than {0} hours ago. I can't mark that session with this information.");
                    return;
                }

                //it's all good, mark the info as done
                latestSession.SessionEnd = DateTimeOffset.Now;

                Regex matchPattern = new Regex(matchPatternText);
                latestSession.ItemsReviewed = matchPattern.Match(userMessage.Content).Groups[1].Value.Parse<int>();

                db.SaveChanges();
            }
        }
    }
}
