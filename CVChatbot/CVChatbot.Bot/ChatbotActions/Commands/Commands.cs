using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// Command to show the list of commands on the server.
    /// </summary>
    public class Commands : UserCommand
    {
        private static class ReflectiveEnumerator
        {
            public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
            {
                var objects = new List<T>();
                foreach (Type type in
                    Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add((T)Activator.CreateInstance(type, constructorArgs));
                }
                return objects;
            }
        }



        public override void RunAction(ChatExchangeDotNet.Message incommingChatMessage, ChatExchangeDotNet.Room chatRoom, InstallationSettings roomSettings)
        {
            var groupedCommands = ChatbotActionRegister.AllChatActions
                .Where(x => x is UserCommand)
                .GroupBy(x => x.GetPermissionLevel());

            var finalMessageLines = new List<string>();
            finalMessageLines.Add("Below is a list of commands for the Close Vote Chat Bot");
            finalMessageLines.Add("");

            foreach (var group in groupedCommands)
            {
                finalMessageLines.Add("{0}".FormatInline(group.Key.ToString()));

                var groupCommandLines = group
                    .OrderBy(x => x.GetActionName())
                    .Select(x => "    {0} - {1}".FormatInline(x.GetActionUsage(), x.GetActionDescription()));

                finalMessageLines.AddRange(groupCommandLines);
                finalMessageLines.Add("");
            }

            var finalMessage = finalMessageLines
                .Select(x => "    " + x)
                .ToCSV(Environment.NewLine);

            chatRoom.PostMessageOrThrow(finalMessage);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        protected override string GetRegexMatchingPattern()
        {
            return "^(show the )?(list of )?(user )?command(s| list)( pl(ease|[sz]))?$";
        }

        public override string GetActionName()
        {
            return "Commands";
        }

        public override string GetActionDescription()
        {
            return "Shows this list.";
        }

        public override string GetActionUsage()
        {
            return "commands";
        }
    }
}
