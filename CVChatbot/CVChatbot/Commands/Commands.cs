using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    /// <summary>
    /// Command to show the list of commands on the server
    /// </summary>
    public class Commands : UserCommand
    {
        public override bool DoesInputTriggerCommand(Message userMessage)
        {
            return userMessage
                .GetContentsWithStrippedMentions()
                .ToLower()
                .Trim()
                == "commands";
        }

        public override void RunCommand(Message userMessage, Room chatRoom)
        {
            var groupedCommands = ReflectiveEnumerator.GetEnumerableOfType<UserCommand>()
                .GroupBy(x => x.GetPermissionLevel());

            string finalMessage = "    Below is a list of commands for the Close Vote Chat Bot" + Environment.NewLine + "    " + Environment.NewLine;

            foreach (var group in groupedCommands)
            {
                finalMessage += "    **" + group.Key.ToString() + "**" + Environment.NewLine;

                finalMessage += group
                    .Select(x => "    " + x.GetHelpText())
                    .OrderBy(x => x)
                    .ToCSV(Environment.NewLine);

                finalMessage += "    " + Environment.NewLine +
                                "    " + Environment.NewLine;
            }

            chatRoom.PostMessage(finalMessage);
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override string GetHelpText()
        {
            return "commands - Shows this list";
        }

        static class ReflectiveEnumerator
        {
            public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
            {
                List<T> objects = new List<T>();
                foreach (Type type in
                    Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add((T)Activator.CreateInstance(type, constructorArgs));
                }
                return objects;
            }
        }
    }
}
