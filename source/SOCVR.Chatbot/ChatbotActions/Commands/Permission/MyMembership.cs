using SOCVR.Chatbot.Database;
using System.Linq;
using ChatExchangeDotNet;
using Microsoft.Data.Entity;
using System.Threading;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal class MyMembership : UserCommand
    {
        public override string ActionDescription => "Shows the permission groups you are a part of.";

        public override string ActionName => "My Membership";

        public override string ActionUsage => "my membership";

        public override PermissionGroup? RequiredPermissionGroup => null;

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => @"^my memberships?$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            using (var db = new DatabaseContext())
            {
                var userData = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == incomingChatMessage.Author.ID);

                var permisionGroupsUserIsIn = userData.Permissions
                    .Select(x => x.PermissionGroup)
                    .ToList();

                if (!permisionGroupsUserIsIn.Any())
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "You're not part of any permission group, which is good, because that means you have less control over me.");
                    return;
                }

                var groupsString = permisionGroupsUserIsIn
                    .Select(x => x.ToString())
                    .CreateOxforCommaListString();

                var message1 = $"First of all, why do you care? Don't you have better things to do with your time then ordering around some chat bot? I've got better things to do with my time, so I'm sure you do as well.";
                var message2 = $"But anyways, because you'd just ask me again if I didn't answer, you are right now in the {groupsString} {(permisionGroupsUserIsIn.Count == 1 ? "group" : "groups")}. Are you happy now?";

                chatRoom.PostReplyOrThrow(incomingChatMessage, message1);
                Thread.Sleep(3000);
                chatRoom.PostReplyOrThrow(incomingChatMessage, message2);
            }
        }
    }
}
