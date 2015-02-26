﻿using CsQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot
{
    /// <summary>
    /// Client responsible to login and run queries on data.stackexchange.com.
    /// Don't use this class directly. Use SedeAccessor instead.
    /// </summary>
    public class SedeClient : IDisposable
    {
        # region Private fields/const(s).

        const string baseUrl = "http://data.stackexchange.com/stackoverflow";

        /// <summary>
        /// Notice that this baby is AppDomain wide used as it is static.
        /// </summary>
        private readonly static CookieContainer cookies = new CookieContainer();

        private bool disposed;
        # endregion



        # region Constructor/destructor.

        public SedeClient(string email, string password)
        {
            ThrowWhen.IsNullOrEmpty(email, "email");
            ThrowWhen.IsNullOrEmpty(password, "password");

            SEOpenIDLogin(email, password);
        }

        ~SedeClient()
        {
            Dispose(false);
        }

        # endregion

        # region Protected methods.
        protected virtual void Dispose(bool dispose)
        {
            if (dispose && !disposed)
            {
                // Clean up managed.
                GC.SuppressFinalize(this);
            }
            // cleanup native
            disposed = true;
        }

        # endregion

        # region Public methods.

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// This runs the Sede Query and gives us the revision Id of the CSV DownloadLink
        /// </summary>
        /// <param name="baseUrl">Where sede and the query live.</param>
        /// <returns>The revision.</returns>
        public string GetSedeQueryCsvRevisionId(string baseUrl)
        {
            ThrowWhen.IsNullOrEmpty(baseUrl, "baseUrl");

            const string SearchTermForRevision = "&quot;revisionId&quot;:";

            var first = Get(baseUrl + "/query/236526/tags-that-can-be-cleared-of-votes#");

            // Tn theory we could parse the result from this html
            // but the data is inside a script tag and we don't fancy yet to parse it.

            // This is expensive...
            var dom = CQ.Create(first);

            // Find the link. This one is slug-ed so this is useless for now.
            string href = dom["#resultSetsButton"][0].Attributes["href"];

            // Find among all inline scripts the one that holds the revisionid.
            var scriptTag = dom["script"].FirstOrDefault(script =>
                !script.HasAttribute("src") &&
                script.InnerText != null &&
                script.InnerText.Contains(SearchTermForRevision));

            string revid = null; // "344267"; // old revision
            if (scriptTag != null)
            {
                var revision = scriptTag.InnerText.IndexOf(SearchTermForRevision) + SearchTermForRevision.Length;
                var nextComma = scriptTag.InnerText.IndexOf(',', revision);
                revid = scriptTag.InnerText.Substring(revision, nextComma - revision).Trim();
            }
            return revid;
        }

        /// <summary>
        /// Retrieves from the sede query the results.
        /// </summary>
        /// <returns>The tagname and the count in a dictionary.</returns>
        public Dictionary<string, int> GetTags()
        {
            // Get in to run first.
            string id = GetSedeQueryCsvRevisionId(baseUrl);

            return GetTags(id);
        }

        /// <summary>
        /// Retrieves from the sede query the results.
        /// </summary>
        /// <returns>The tagname and the count in a dictionary.</returns>
        public Dictionary<string, int> GetTags(string csvRevId)
        {
            ThrowWhen.IsNullOrEmpty(csvRevId, "csvRevId");

            // For collecting the result.
            Dictionary<string, int> tags = null;

            // This gets a text/csv content
            // tag, count
            // "java", "200"
            // "php", "120"
            var csv = GetCSVQuery(baseUrl + "/csv/" + csvRevId);

            tags = new Dictionary<string, int>();
            var header = true; // Skip the header.

            // Split the returned string on each line.
            foreach (var line in csv.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                if (header)
                {
                    header = false;
                }
                else
                {
                    // Split a line in fields.
                    var fields = line.Split(',');

                    // First field is the tag enclosed in " which we remove.
                    var tag = fields[0].Replace("\"", "");

                    // Parse the count from the second field.
                    int cnt;
                    Int32.TryParse(fields[1].Replace("\"", ""), out cnt);

                    tags.Add(tag, cnt);
                }
            }

            return tags;
        }
        # endregion



        # region Private methods.

        /// <summary>
        /// GET from the url whatever is returned as a string.
        /// This method uses/fills the shared CookieContainer.
        /// </summary>
        private string Get(string url)
        {
            var req = HttpWebRequest.CreateHttp(url);
            req.Method = "GET";
            req.AllowAutoRedirect = true;
            req.CookieContainer = cookies;
            var resp = (HttpWebResponse)req.GetResponse();
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// This seems a thin wrapper but this here for future use.
        /// </summary>
        private string GetCSVQuery(string query)
        {
            return Get(query);
        }
        /// <summary>
        /// POST to the url the urlencoded data and return the contents as a string.
        /// This method uses/fills the shared CookieContainer.
        /// </summary>
        private string Post(string url, string data)
        {
            var req = HttpWebRequest.CreateHttp(url);
            req.Method = "POST";
            req.CookieContainer = cookies;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write(data);
                sw.Flush();
            }
            var resp = (HttpWebResponse)req.GetResponse();
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }


        /// <summary>
        /// Perform an login for the SE openID provider.
        /// Notice that when you run this from the BOT
        /// you are already logged-in,
        /// we only get the Cookies.
        /// </summary>
        private void SEOpenIDLogin(string email, string password)
        {
            // Do a Get to retrieve the cookies.
            var start = Get("https://openid.stackexchange.com/account/login");

            // If we find no fkey in html.
            string fkey = CQ.Create(start).GetInputValue("fkey");

            // We are already logged in.
            if (!String.IsNullOrEmpty(fkey))
            {
                // We found an fkey, use it to login via openid.
                var data = "email=" + Uri.EscapeDataString(email) +
                           "&password=" + Uri.EscapeDataString(password) +
                           "&fkey=" + fkey;

                var res = Post("https://openid.stackexchange.com/account/login/submit", data);

                if (String.IsNullOrEmpty(res)) // Better error check, because this is nonsense.
                {
                    throw new Exception("Could not login using the specified OpenID credentials. Have you entered the correct credentials and have an active internet connection?");
                }
            }
        }

        # endregion
    }
}
