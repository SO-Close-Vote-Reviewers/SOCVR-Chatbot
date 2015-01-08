using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Commands
{
    public class Alive : UserCommand
    {
        public override bool DoesInputTriggerCommand(Message userMessage)
        {
            List<string> acceptablePhrases = new List<string>()
            {
                "alive",
                "you alive",
                "you alive?",
                "are you alive?",
            };

            return userMessage.Content
                .ToLower()
                .Trim()
                .In(acceptablePhrases);
        }

        public override void RunCommand(Message userMessage, Room chatRoom)
        {
            List<string> responsePhrases = new List<string>()
            {
                "I'm alive and kicking!",
                "Still here you guys!",
                "I'm not dead yet!",
            };

            var phrase = responsePhrases.PickRandom();

            chatRoom.PostReply(userMessage, phrase);
        }
    }

    public static class EnumerableExtension
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}
