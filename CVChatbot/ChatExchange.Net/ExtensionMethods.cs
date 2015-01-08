using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using CsQuery;



namespace ChatExchangeDotNet
{
    public static class ExtensionMethods
    {
        private static readonly Regex hasMention = new Regex(@"^:\d*?\s|@\S{3,}?\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex isReply = new Regex(@"^:\d*?\s", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public static List<Cookie> GetCookies(this CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, container, new object[] { });

            foreach (var key in table.Keys)
            {
                Uri uri;

                var domain = key as string;

                if (domain == null) { continue; }

                if (domain.StartsWith("."))
                {
                    domain = domain.Substring(1);
                }

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false) { continue; }

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }
        
            return cookies;
        }

        public static string GetFkey(this CQ input)
        {
            var fkeyE = input["input"].FirstOrDefault(e => e.Attributes["name"] != null && e.Attributes["name"] == "fkey");

            return fkeyE == null ? null : fkeyE.Attributes["value"];
        }

        public static List<Message> GetMessagesByUser(this IEnumerable<Message> messages, User user)
        {
            if (user == null) { throw new ArgumentNullException("user"); }

            return messages.GetMessagesByUser(user.ID);
        }

        public static List<Message> GetMessagesByUser(this IEnumerable<Message> messages, int userID)
        {
            if (messages == null) { throw new ArgumentNullException("messages"); }

            var userMessages = new List<Message>();

            foreach (var m in messages)
            {
                if (m.AuthorID == userID)
                {
                    userMessages.Add(m);
                }
            }

            return userMessages;
        }

        public static string StripMention(this string input, string replaceWith = "")
        {
            return hasMention.Replace(input, replaceWith);
        }

        public static bool IsReply(this string message, bool includeMention = false)
        {
            return includeMention ? hasMention.IsMatch(message) : isReply.IsMatch(message);
        }
    }
}
