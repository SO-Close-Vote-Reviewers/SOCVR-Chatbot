using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Commands
{
    public class Alive : UserCommand
    {
        public async override Task<bool> DoesInputTriggerCommandAsync(Message userMessage)
        {
            return await Task.Run(() => userMessage.Content.ToLower().Trim() == "alive");
        }

        public async override Task RunCommandAsync(Message userMessage, Room chatRoom)
        {
            await Task.Run(() => RunCommand(userMessage, chatRoom));
        }

        private void RunCommand(Message userMessage, Room chatRoom)
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
