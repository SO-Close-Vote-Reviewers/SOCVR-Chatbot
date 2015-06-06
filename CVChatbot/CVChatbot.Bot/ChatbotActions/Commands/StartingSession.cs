using CVChatbot.Bot.Database;
using System.Collections.Generic;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class StartingSession : UserCommand
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var message = "You don't need to run this command anymore! When you start reviewing I should notice it and record the start of the record.";
            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^(?:i'?m )?start(ing|ed)(?: now)?$";
        }

        public override string GetActionName()
        {
            return "Starting";
        }

        public override string GetActionDescription()
        {
            return "Unnecessary - Informs the chatbot that you are starting a new review session.";
        }

        public override string GetActionUsage()
        {
            return "starting";
        }
    }
}
