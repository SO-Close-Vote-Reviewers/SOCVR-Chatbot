using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CsQuery;
using ChatExchangeDotNet;
using System.Net;

namespace CVChatbot
{
    /// <summary>
    /// helper to throw exceptions to enforce Incoming contracts
    /// </summary>
    public static class ThrowWhen
    {
        /// <summary>
        /// throws an argument exception if value is null or empty
        /// </summary>
        /// <param name="value">a value</param>
        /// <param name="paramName">friendly name of value</param>
        public static void IsNullOrEmpty(string value, string paramName)
        {
            if (String.IsNullOrEmpty(value)) 
            { 
                throw new ArgumentException(String.Format("'{0}' must not be null or empty.",paramName) , paramName); 
            }
        }
    }

    /// <summary>
    /// client responsible to login and run queries on data.stackexchange.com
    /// </summary>
    public class SedeClient : IDisposable
    {
        private readonly string email;
        private readonly string password;
        private bool disposed;

        // notice that this baby is AppDomain wide used as it is static...
        private readonly static  CookieContainer cookies = new CookieContainer();

        public SedeClient(string email, string password)
        {
            ThrowWhen.IsNullOrEmpty(email, "email");
            ThrowWhen.IsNullOrEmpty(password, "password");

            this.email = email;
            this.password = password;

            SEOpenIDLogin(email, password);
        }

        ~SedeClient()
        {
            if (disposed) { return; }

            disposed = true;
        }

        /// <summary>
        /// Retrieves from the sede query the results 
        /// </summary>
        /// <returns>the tagname and the count in a dictionary</returns>
        public Dictionary<string, int> GetTags()
        {
            

            string baseUrl = "http://data.stackexchange.com/stackoverflow";
            // get in to run first
            string id = GetSedeQueryCsvRevisionId(baseUrl);

            // for collecting the result 
            Dictionary<string, int> tags = null;
            if (id != null)
            {
                // this gets a text/csv content
                // tag, count
                // "java", "200"
                // "php", "120"
                var csv = GetCSVQuery(baseUrl + "/csv/" + id);

                tags = new Dictionary<string, int>();
                var header = true; // skip the header
                // split the returned string on each line
                foreach (var line in csv.Split(new string[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (header)
                    {
                        header = false;
                    }
                    else
                    {
                        // split a line in fields
                        var fields = line.Split(',');
                        // first field is the tag enclosed in " which we remove
                        var tag = fields[0].Replace("\"", "");
                        // parse the count from the second field
                        int cnt;
                        Int32.TryParse(fields[1].Replace("\"", ""), out cnt);

                        tags.Add(tag, cnt);
                    }
                }
            }
            return tags;
        }

        /// <summary>
        /// This runs the Sede Query and gives us the revision Id of the CSV DownloadLink
        /// </summary>
        /// <param name="baseUrl">where sede and the query live</param>
        /// <returns>the revision</returns>
        private string GetSedeQueryCsvRevisionId(string baseUrl)
        {
            const string SearchTermForRevision = "&quot;revisionId&quot;:";

            var first = Get(baseUrl + "/query/236526/tags-that-can-be-cleared-of-votes#");

            // in theory we could parse the result from this html
            // but the data is inside a script tag and I don't fancy yet to parse it

            var dom = CQ.Create(first); // this is expensive ... 
            // find the link
            // this one is slug-ed so this is useless for now
            string href = dom
                .Find("#resultSetsButton")
                .First()
                .Attr("href");

            //find among all inline scripts the one that holds the revisionid
            var scriptTag = dom
                .Find("script")
                .Where(script => !script.HasAttribute("src") &&
                        script.InnerText != null &&
                        script.InnerText.Contains(SearchTermForRevision))
                .FirstOrDefault();

            string revid = null; // "344267"; // old revision
            if (scriptTag != null)
            {
                var revision = scriptTag.InnerText.IndexOf(SearchTermForRevision) + SearchTermForRevision.Length;
                var nextComma = scriptTag.InnerText.IndexOf(',', revision);
                revid = scriptTag.InnerText.Substring(revision, nextComma - revision).Trim();
            }
            return revid;
        }

        // this seems a thin wrapper but this here for future use
        private string GetCSVQuery(string query)
        {
            return Get(query);
        }

        public void Dispose()
        {
            if (disposed) { return; }

            // clean up
            GC.SuppressFinalize(this);
            disposed = true;
        }

        // GET from the url whatever is returned as a string
        // notice that this method uses/ fills the shared CookieContainer 
        private string Get(string url)
        {
            var req = HttpWebRequest.CreateHttp(url);
            req.Method = "GET";
            req.AllowAutoRedirect = true;
            req.CookieContainer = cookies;
            var resp = (HttpWebResponse)req.GetResponse();
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        // POST to the url the urlencoded data and return the contents as a string
        // notice that this method uses/ fills the shared CookieContainer
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
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        // perform an login for the SE openID provider
        // notice that when you run this from the BOT
        // you are already logged-in,
        // we only get the Cookies 
        private void SEOpenIDLogin(string email, string password)
        {
            // do a Get to retrieve the cookies

            var start = Get("https://openid.stackexchange.com/account/login");

            // if we find no fkey in html ...
            string fkey = CQ.Create(start).GetInputValue("fkey");
            // ... we are already logged in...
            if (!String.IsNullOrEmpty(fkey))
            {
                // ... we found an fkey, use it to login in the openid
                var data = "email=" + Uri.EscapeDataString(email) +
                           "&password=" + Uri.EscapeDataString(password) +
                           "&fkey=" + fkey;

                var res = Post("https://openid.stackexchange.com/account/login/submit", data);

                if (String.IsNullOrEmpty(res)) // better error check, because this is nonsense
                {
                    throw new Exception("Could not login using the specified OpenID credentials. Have you entered the correct credentials and have an active internet connection?");
                }
            }
        }
    }

    //stolen from CE.Net
    internal static class CQExtension
    {
        //On a CQ dom find an <input name="foo" value="bar" > with the name foo and return bar or null for no match
        internal static string GetInputValue(this CQ input, string elementName)
        {
            var fkeyE = input["input"].FirstOrDefault(e => e.Attributes["name"] != null && e.Attributes["name"] == elementName);

            return fkeyE == null ? null : fkeyE.Attributes["value"];
        }
    }
}