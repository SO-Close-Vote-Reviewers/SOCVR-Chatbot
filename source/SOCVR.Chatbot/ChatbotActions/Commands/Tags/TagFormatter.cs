using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tags
{
    internal static class TagFormatter
    {
        public static string CreateQueueLinkedTag(string tagName)
        {
            var linkUrl = $"http://stackoverflow.com/review/close?filter-tags={tagName}";
            return $"[tagName]({linkUrl})";
        }

        public static string CreateQueueLinkedTagWithReviewCount(string tagName, int reviewCount)
        {
            var linkString = CreateQueueLinkedTag(tagName);
            return $"{linkString} `{reviewCount}`";
        }
    }
}
