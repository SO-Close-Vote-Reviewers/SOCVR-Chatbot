/*
 * ChatExchange.Net. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CVChatbot.Bot.ChatbotActions
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
        public static List<ChatbotAction> AllChatActions
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
        public static string GetChatBotActionUsage<TAction>()
            where TAction : ChatbotAction
        {
            return AllChatActions
                .Single(x => x is TAction)
                .GetActionUsage();
        }

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
    }
}
