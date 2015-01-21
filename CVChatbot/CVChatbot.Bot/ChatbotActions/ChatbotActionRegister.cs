using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions
{
    public static class ChatbotActionRegister
    {
        public static List<ChatbotAction> AllChatActions
        {
            get
            {
                return ReflectiveEnumerator
                    .GetEnumerableOfType<ChatbotAction>()
                    .ToList();
            }
        }

        public static string GetChatBotActionUsage<TAction>()
            where TAction : ChatbotAction
        {
            return AllChatActions
                .Where(x => x is TAction)
                .Single()
                .GetActionUsage();
        }

        private static class ReflectiveEnumerator
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
