using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SOCVR.Chatbot.ChatbotActions
{
    /// <summary>
    /// Holds a list of instances that inherit from ChatbotAction.
    /// This is essentially the UserCommand and Trigger listing.
    /// </summary>
    public static class ChatbotActionRegister
    {
        /// <summary>
        /// Returns list where each element is an instance of a different class that inherits ChatBotAction and is not abstract.
        /// Use this value to loop through all the UserCommands and Triggers in the program.
        /// </summary>
        internal static List<ChatbotAction> AllChatActions
        {
            get
            {
                return ReflectiveEnumerator
                    .GetEnumerableOfType<ChatbotAction>()
                    .ToList();
            }
        }

        /// <summary>
        /// Return the usage for a given action.
        /// </summary>
        /// <typeparam name="TAction">A type that inherits ChatbotAction.</typeparam>
        /// <returns></returns>
        internal static string GetChatBotActionUsage<TAction>()
            where TAction : ChatbotAction
        {
            return AllChatActions
                .Single(x => x is TAction)
                .ActionUsage;
        }

        internal static class ReflectiveEnumerator
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
    }
}
