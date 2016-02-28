using System;
using System.Linq;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tracking
{
    internal abstract class OptTrackingCommand : UserCommand
    {
        public override sealed void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var user = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == incomingChatMessage.Author.ID);

                if (user.OptInToReviewTracking == GetOptValue())
                {
                    //user already has that opt value

                    //if you are in the reviews group your LastTrackingPreferenceChange must be set
                    var deltaTime = DateTimeOffset.UtcNow - user.LastTrackingPreferenceChange.Value;

                    var replyMessage = $"You are already {GetPastTencePhrase()} {TrackingPhrasePrefix()} tracking, and have been in this state for {deltaTime.ToUserFriendlyString()}. ";
                    replyMessage += $"You may switch your preference by running `{OppositeCommandUsage()}`";
                    chatRoom.PostReplyOrThrow(incomingChatMessage, replyMessage);
                    return;
                }

                //flip the setting and update the LastTrackingPreferenceChange value
                user.OptInToReviewTracking = GetOptValue();
                user.LastTrackingPreferenceChange = DateTimeOffset.UtcNow;
                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, $"You have {GetPastTencePhrase()} {TrackingPhrasePrefix()} tracking, and will remain this way until you run `{OppositeCommandUsage()}`.");
            }
        }

        /// <summary>
        /// Returns true or false depending on if the child class is Opt-in or Opt-out.
        /// This is what will be compared to OptInToReviewTracking in the database.
        /// </summary>
        /// <returns></returns>
        protected abstract bool GetOptValue();

        /// <summary>
        /// "opted-in" or "opted-out".
        /// </summary>
        /// <returns></returns>
        protected abstract string GetPastTencePhrase();

        /// <summary>
        /// "to" or "of".
        /// You are already opted-[in/out] [of/to] tracking...
        /// </summary>
        /// <returns></returns>
        protected abstract string TrackingPhrasePrefix();

        protected abstract string OppositeCommandUsage();
    }
}
