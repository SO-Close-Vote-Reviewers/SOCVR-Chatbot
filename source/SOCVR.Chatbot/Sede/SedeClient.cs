using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CsQuery;
using ServiceStack.Text;

namespace SOCVR.Chatbot.Sede
{
    /// <summary>
    /// Client responsible to login and run queries on data.stackexchange.com.
    /// Don't use this class directly. Use SedeAccessor instead.
    /// </summary>
    public class SedeClient : IDisposable
    {
        # region Private fields/const(s).

        const string host = "http://data.stackexchange.com";
        const string baseUrl = "http://data.stackexchange.com/stackoverflow";
        const string baseUrlFormat = "http://data.stackexchange.com{0}";
        const string baseUrlJobFormat = "http://data.stackexchange.com/query/job/{0}";
        const string baseUrlQueryFormat = "http://data.stackexchange.com/stackoverflow/query/{0}";

        /// <summary>
        /// Notice that this baby is AppDomain wide used as it is static.
        /// </summary>
        private readonly static CookieContainer cookies = new CookieContainer();

        private bool disposed;

        // holds the login method for when we are logged out
        Action login;
        # endregion

        

        # region Constructor/destructor.

        public SedeClient(string email, string password)
        {
            ThrowWhen.IsNullOrEmpty(email, "email");
            ThrowWhen.IsNullOrEmpty(password, "password");

            login = () => SEOpenIDLogin(email, password);

            login.Invoke();
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
        public string GetSedeQueryRunUrl(int queryid)
        {
            // get the initial query page 
            var data = Get(String.Format(baseUrlQueryFormat, queryid));
            return GetFormActionUrl(data);
        }



        /// <summary>
        /// Retrieves from the sede query the results.
        /// </summary>
        /// <returns>The tagname and the count in a dictionary.</returns>
        public Dictionary<string, int> GetTags(string url)
        {
            ThrowWhen.IsNullOrEmpty(url, "action url");
            // Get in to run first.
            var jobResult = GetQueryResult(url);

            return GetTags(jobResult);
        }

        /// <summary>
        /// Retrieves from the SEDE query the results.
        /// </summary>
        /// <returns>The tag's name and the count in a Dictionary.</returns>
        public Dictionary<string, int> GetTags(JobResult jobResult)
        {
            var dict = new Dictionary<string, int>();
            if (jobResult == null)
            {
                dict.Add("No tags found", 42);
            }
            else
            {
                if (jobResult.ResultSets != null &&
                    jobResult.ResultSets.Length > 0)
                {
                    foreach (var row in jobResult.ResultSets[0].Rows)
                    {
                        int value;
                        Int32.TryParse(row.Values.First(), out value);
                        dict.Add(row.Keys.First(), value);
                    }
                }
                else
                {
                    dict.Add("No resultsets found", 42);
                }
            }

            return dict;
        }
        # endregion



        # region Private methods.

        /// <summary>
        /// Formaction is the URL posted to if the query is run. This includes the revision.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetFormActionUrl(string data)
        {
            const string formTag = @"<form id=""runQueryForm"" action=""";
            // No fancy regex here.
            string result = null;
            var postUrlAction = data.IndexOf(formTag);
            if (postUrlAction > -1)
            {
                var closingQuote = data.IndexOf('"', postUrlAction + formTag.Length);
                if (closingQuote > -1)
                {
                    result = data.Substring(postUrlAction + formTag.Length, closingQuote - (postUrlAction + formTag.Length));
                }
            }
            return result;
        }

        /// <summary>
        /// The query result is retrieved by POSTING the formaction with no data.
        /// </summary>
        /// <param name="url">The formaction URL.</param>
        /// <returns>A JobResult.</returns>
        private JobResult GetQueryResult(string url)
        {
            JobResult jobResult = null;
            for (int loginAttemtps = 0; loginAttemtps < 3; loginAttemtps++)
            {
                using (var jobStream = PostStream(String.Format(baseUrlFormat, url), null))
                {

                    jobResult = JsonSerializer.DeserializeFromStream<JobResult>(jobStream);
                    if (jobResult.Captcha)
                    {
                        Console.WriteLine($"Captcha requested! Not logged in? Attempt: {loginAttemtps}");
                        login.Invoke();
                    }
                    else
                    {
                        // If a job is started on the server
                        // we have to poll for the result.
                        // This call might block while polling but 
                        // if the result is already there we return immediately.
                        jobResult = GetQueryResultByPolling(jobResult);

                        if (jobResult.ResultSets != null
                            && jobResult.ResultSets.Length > 0)
                        {
                            Console.WriteLine(jobResult.ResultSets[0].Rows.Count);
                        }
                        else
                        {
                            Console.WriteLine("Hmm, no result sets...");
                            // Maybe an error.
                            Console.WriteLine(jobResult.Error);
                        }
                        // we don't need to do any next attempts, we have something
                        break;
                    }
                }
            }
            return jobResult;
        }

        // This call is blocking, while doing work on a timer thread.
        private JobResult GetQueryResultByPolling(JobResult jobResult)
        {
            // We need to poll.
            if (jobResult.Running)
            {
                var serializeTimer = new Object();
                var done = new ManualResetEvent(false); // Wait handle.
                // We use a timer that runs every second.
                using (var timer = new Timer((state) =>
                {
                    // Make sure we are single threaded.
                    lock (serializeTimer)
                    {
                        // If we are not running, don't override our result.
                        if (jobResult.Running)
                        {
                            // Poll result.
                            Console.WriteLine(jobResult.Job_id);
                            using (var pollStream = GetAsStream(String.Format(baseUrlJobFormat, jobResult.Job_id)))
                            {
                                jobResult = JsonSerializer.DeserializeFromStream<JobResult>(pollStream);
                            }
                        }
                        // If we have a result, get out!
                        if (!jobResult.Running)
                        {
                            done.Set(); // Signal main thread.
                        }
                    }
                }
                    ))
                {
                    timer.Change(100, 1000);

                    bool signaled = done.WaitOne(new TimeSpan(0, 1, 0));
                    if (!signaled)
                    {
                        Console.WriteLine("no response in 1 minute");
                    }
                }
            }
            return jobResult;
        }

        /// <summary>
        /// GET from the url whatever is returned as a string.
        /// This method uses/fills the shared CookieContainer.
        /// </summary>
        private string Get(string url)
        {
            using (var sr = new StreamReader(GetAsStream(url)))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Get the raw stream from the http response.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The stream.</returns>
        private static Stream GetAsStream(string url)
        {
            var req = HttpWebRequest.CreateHttp(url);
            req.Method = "GET";
            req.AllowAutoRedirect = true;
            req.CookieContainer = cookies;
            var resp = (HttpWebResponse)req.GetResponse();
            return resp.GetResponseStream();
        }

        /// <summary>
        /// This seems a thin wrapper but this here for future use.
        /// </summary>
        private string GetCSVQuery(string query) => Get(query);

        /// <summary>
        /// POST to the url the urlencoded data and return the contents as a string.
        /// This method uses/fills the shared CookieContainer.
        /// </summary>
        private string Post(string url, string data)
        {
            using (var resp = PostStream(url, data))
            {
                using (var sr = new StreamReader(resp))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Posts and returns the stream
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static Stream PostStream(string url, string data)
        {
            var req = HttpWebRequest.CreateHttp(url);
            req.Method = "POST";
            req.CookieContainer = cookies;
            if (!String.IsNullOrEmpty(data))
            {
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;
                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    sw.Write(data);
                    sw.Flush();
                }
            }
            else
            {
                req.ContentLength = 0;
            }
            var resp = (HttpWebResponse)req.GetResponse();
            return resp.GetResponseStream();
        }



        /// <summary>
        /// Perform an login for the SE openID provider.
        /// Notice that when you run this from the BOT
        /// you are already logged-in,
        /// we only get the Cookies.
        /// </summary>
        private void SEOpenIDLogin(string email, string password)
        {
            //post
            var start = Post("http://data.stackexchange.com/user/authenticate?returnurl=/", "openid=https%3A%2F%2Fopenid.stackexchange.com%2F");

            var html = CQ.Create(start);
            // If we find no fkey or session in html.
            string fkey = html.GetInputValue("fkey");
            string session = html.GetInputValue("session");

            // We are already logged in.
            if (!String.IsNullOrEmpty(fkey))
            {
                // We found an fkey, use it to login via openid.
                var data = "email=" + Uri.EscapeDataString(email) +
                           "&password=" + Uri.EscapeDataString(password) +
                           "&fkey=" + fkey +
                           "&session=" + session;

                var res = Post("https://openid.stackexchange.com/account/login/submit", data);

                if (String.IsNullOrEmpty(res)) // Better error check, because this is nonsense.
                {
                    throw new Exception("Could not login using the specified OpenID credentials. Have you entered the correct credentials and have an active internet connection?");
                }
            }
        }

        # endregion

        public class ResultSet
        {
            public JsonArrayObjects Rows { get; set; }
        }

        public class JobResult
        {
            public string Line { get; set; }
            public string Error { get; set; }
            public bool Captcha { get; set; }
            public bool Running { get; set; }
            public string Job_id { get; set; }
            public int RevisionId { get; set; }
            public ResultSet[] ResultSets { get; set; }
        }

    }
}
