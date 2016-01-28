using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands
{
    /// <summary>
    /// Command to show the list of commands on the server.
    /// </summary>
    internal class Commands : UserCommand
    {
        public override string ActionDescription => "Shows this list.";

        public override string ActionName => "Commands";

        public override string ActionUsage => "commands";

        public override PermissionGroup? RequiredPermissionGroup => null;

        protected override string RegexMatchingPattern => "^(show the )?(list of )?(user )?command(s| list)( pl(ease|[sz]))?$";

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

        public override void RunAction(ChatExchangeDotNet.Message incomingChatMessage, ChatExchangeDotNet.Room chatRoom)
        {
            var groupedCommands = ChatbotActionRegister.AllChatActions
                .Where(x => x is UserCommand)
                .GroupBy(x => x.RequiredPermissionGroup);

            var finalMessageLines = new List<string>();
            finalMessageLines.Add("Below is a list of commands for the Close Vote Chat Bot");
            finalMessageLines.Add("");

            foreach (var group in groupedCommands)
            {
                finalMessageLines.Add(group.Key.ToString());

                var groupCommandLines = group
                    .OrderBy(x => x.ActionName)
                    .Select(x => $"    {x.ActionUsage} - {x.ActionDescription}");

                finalMessageLines.AddRange(groupCommandLines);
                finalMessageLines.Add("");
            }

            var finalMessage = finalMessageLines
                .Select(x => "    " + x)
                .ToCSV(Environment.NewLine);

            chatRoom.PostMessageOrThrow(finalMessage);
        }
    }
}
