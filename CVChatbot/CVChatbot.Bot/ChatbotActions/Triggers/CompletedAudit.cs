using ChatExchangeDotNet;
using CVChatbot.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    public class CompletedAudit : Trigger
    {
        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var chatUser = chatRoom.GetUser(incommingChatMessage.Author.ID);
            var tagName = GetRegexMatchingObject()
                    .Match(GetMessageContentsReadyForRegexParsing(incommingChatMessage))
                    .Groups[1]
                    .Value;

            var da = new DatabaseAccessor(roomSettings.DatabaseConnectionString);
            da.InsertCompletedAuditEntry(incommingChatMessage.Author.ID, tagName);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Registered;
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^passed\s+(?:an?\s+)?(\S+)\s+audit$";
        }

        public override string GetActionName()
        {
            return "Completed Audit";
        }

        public override string GetActionDescription()
        {
            return null;
        }

        public override string GetActionUsage()
        {
            return null;
        }
    }
}
