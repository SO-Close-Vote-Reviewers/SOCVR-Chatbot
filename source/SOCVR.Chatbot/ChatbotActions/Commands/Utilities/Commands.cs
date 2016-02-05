using Microsoft.Data.Entity;
using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Utilities
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

        public override bool UserMustBeInAnyPermissionGroupToRun => false;

        protected override string RegexMatchingPattern => "^commands(?: (full))?$";

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
            var userCommands = ChatbotActionRegister.AllChatActions
                .Where(x => x is UserCommand);

            var showAllCommands = GetRegexMatchingObject()
                .Match(incomingChatMessage.Content)
                .Groups[1]
                .Success;

            if (showAllCommands)
            {
                var groupedCommands = ChatbotActionRegister.AllChatActions
                    .Where(x => x is UserCommand)
                    .GroupBy(x => x.RequiredPermissionGroup);

                var finalMessageLines = new List<string>();
                finalMessageLines.Add("Below is a list of commands for the Close Vote Chat Bot");
                finalMessageLines.Add("");

                foreach (var group in groupedCommands)
                {
                    var permissionGroupName = group.Key != null
                        ? group.Key.ToString()
                        : "Public";

                    finalMessageLines.Add(permissionGroupName);

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
            else
            {
                chatRoom.PostReplyOrThrow(incomingChatMessage, "Here is a list of commands you have permission to run:");

                using (var db = new DatabaseContext())
                {
                    var permissionsForUser = db.Users
                        .Include(x => x.Permissions)
                        .Single(x => x.ProfileId == incomingChatMessage.Author.ID)
                        .Permissions
                        .Select(x => x.PermissionGroup)
                        .ToList();

                    var commandMessage = userCommands
                        .Where(x => x.RequiredPermissionGroup == null || x.RequiredPermissionGroup.Value.In(permissionsForUser))
                        .OrderBy(x => x.ActionName)
                        .Select(x => $"    {x.ActionUsage} - {x.ActionDescription}")
                        .ToCSV(Environment.NewLine);

                    chatRoom.PostMessageOrThrow(commandMessage);
                }
            }
        }
    }
}
