using SOCVR.Chatbot.Sede;
using System.Linq;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.Configuration;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    /// <summary>
    /// Implements the current command that takes the first tag from the SEDE query and post it to the room.
    /// </summary>
    internal class CurrentTag : UserCommand
    {
        public override string ActionDescription =>
            "Get the tag that has the most amount of manageable close queue items from the SEDE query.";

        public override string ActionName => "Current Tag";

        public override string ActionUsage => "current tag";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => @"^(what is the )?current tag( pl(ease|[sz]))?\??$";

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var tags = SedeAccessor.GetTags(chatRoom, ConfigurationAccessor.LoginEmail, ConfigurationAccessor.LoginPassword);

            if (tags == null)
            {
                chatRoom.PostReplyOrThrow(incomingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            string dataMessage;
            if (tags != null)
            {
                dataMessage = $"The current tag is [tag:{tags.First().Key}] with {tags.First().Value} known review items.";
            }
            else
            {
                dataMessage = "I couldn't find any tags! Either the query is empty or something bad happened.";
            }

            chatRoom.PostReplyOrThrow(incomingChatMessage, dataMessage);
        }
    }
}
