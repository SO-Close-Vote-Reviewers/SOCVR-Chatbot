using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tracking
{
    class OptIn : UserCommand
    {
        public override string ActionDescription => "Tells the bot to resume tracking your close vote reviewing.";

        public override string ActionName => "Opt In";

        public override string ActionUsage => "opt in";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^opt[ -]in$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var user = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == incomingChatMessage.Author.ID);

                if (user.OptInToReviewTracking == true)
                {
                    //user is already opted-in

                    //if you are in the reviews group your LastTrackingPreferenceChange must be set
                    var deltaTime = DateTimeOffset.UtcNow - user.LastTrackingPreferenceChange.Value;

                    var replyMessage = $"You are already opted-in to tracking, and have been in this state for {deltaTime.ToUserFriendlyString()}.";
                    chatRoom.PostReplyOrThrow(incomingChatMessage, replyMessage);
                    return;
                }

                //flip the setting and update the LastTrackingPreferenceChange value
                user.OptInToReviewTracking = false;
                user.LastTrackingPreferenceChange = DateTimeOffset.UtcNow;
                db.SaveChanges();

                chatRoom.PostReplyOrThrow(incomingChatMessage, "You have been opted-in to tracking, and will remain this way until you run `opt out`.");
            }
        }
    }
}
