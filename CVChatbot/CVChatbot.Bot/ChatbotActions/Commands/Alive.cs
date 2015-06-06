using System.Collections.Generic;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Alive : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var responsePhrases = new List<string>()
            {
                "I'm alive and kicking!",
                "Still here you guys!",
                "I'm not dead yet!",
                "I feel... happy!",
                "I think I'll go for a walk...",
                "I don't want to go on the cart!",
                "I feel fine.",
            };

            var phrase = responsePhrases.PickRandom();

            chatRoom.PostReplyOrThrow(incommingChatMessage, phrase);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:(?:are )?you )?(alive|still there|(still )?with us)\??$";
        }

        public override string GetActionName()
        {
            return "Alive";
        }

        public override string GetActionDescription()
        {
            return "A simple ping command to test if the bot is running.";
        }

        public override string GetActionUsage()
        {
            return "alive";
        }
    }
}
