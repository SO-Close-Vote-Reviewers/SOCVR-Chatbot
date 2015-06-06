using System.Linq;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Implements the current command that takes the first tag from the SEDE query and post it to the room.
    /// </summary>
    public class CurrentTag : UserCommand
    {
        public override string GetActionDescription()
        {
            return "Get the tag that has the most amount of manageable close queue items from the SEDE query.";
        }

        public override string GetActionName()
        {
            return "Current Tag";
        }

        public override string GetActionUsage()
        {
            return "current tag";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        /// <summary>
        /// Outputs the tag.
        /// </summary>
        /// <param name="incommingChatMessage"></param>
        /// <param name="chatRoom"></param>
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var tags = SedeAccessor.GetTags(chatRoom, roomSettings.Email, roomSettings.Password);

            if (tags == null)
            {
                chatRoom.PostReplyOrThrow(incommingChatMessage, "My attempt to get tag data returned no information. This could be due to the site being down or blocked for me, or a programming error. Try again in a few minutes, or tell the developer if this happens often.");
                return;
            }

            string dataMessage;
            if (tags != null)
            {
                dataMessage = "The current tag is [tag:{0}] with {1} known review items.".FormatInline(tags.First().Key, tags.First().Value);
            }
            else
            {
                dataMessage = "I couldn't find any tags! Either the query is empty or something bad happened.";
            }

            chatRoom.PostReplyOrThrow(incommingChatMessage, dataMessage);
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(what is the )?current tag( pl(ease|[sz]))?\??$";
        }
    }
}
