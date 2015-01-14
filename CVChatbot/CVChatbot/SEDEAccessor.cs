using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CsQuery;
using ChatExchangeDotNet;

namespace CVChatbot
{
    public class SedeClient : IDisposable
    {
        private readonly string username;
        private readonly string email;
        private readonly string password;
        private bool disposed;

        public SedeClient(string username, string email, string password)
        {
            if (String.IsNullOrEmpty(email)) { throw new ArgumentException("'email' must not be null or empty.", "email"); }
            if (String.IsNullOrEmpty(password)) { throw new ArgumentException("'password' must not be null or empty.", "password"); }

            this.username = username;
            this.email = email;
            this.password = password;

            SEOpenIDLogin(email, password);
        }

        ~SedeClient()
        {
            if (disposed) { return; }

            disposed = true;
        }

        public Dictionary<string, int> GetTags()
        {
            // get in to eun first
            var first = GetRunQuery(@"http://data.stackexchange.com/stackoverflow/query/236526/tags-that-can-be-cleared-of-votes#");
            // no error checking whatsoever !!!!!!! :(
            var csv = GetCSVQuery("http://data.stackexchange.com/stackoverflow/csv/344267");

            var tags = new Dictionary<string, int>();
            var header = true;
            foreach (var line in csv.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                var flds = line.Split(',');
                var tag = flds[0].Replace("\"", "");
                int cnt;
                Int32.TryParse(flds[1].Replace("\"", ""), out cnt);
                if (header)
                {
                    header = false;
                }
                else
                {
                    tags.Add(tag, cnt);
                }
            }
            return tags;
        }

        private string GetRunQuery(string query)
        {
            var resp = RequestManager.SendGETRequest(query);
            var str = resp.GetResponseStream();
            using (var ms = new MemoryStream())
            {
                str.CopyTo(ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private string GetCSVQuery(string query)
        {
            var resp = RequestManager.SendGETRequest(query);
            var str = resp.GetResponseStream();
            using (var ms = new MemoryStream())
            {
                str.CopyTo(ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public void Dispose()
        {
            if (disposed) { return; }

            // clean up
            GC.SuppressFinalize(this);
            disposed = true;
        }

        private void SEOpenIDLogin(string email, string password)
        {
            var getRes = RequestManager.SendGETRequest("https://openid.stackexchange.com/account/login");

            if (getRes == null) { throw new Exception("Could not get OpenID fkey. Do you have an active internet connection?"); }

            var getResContent = RequestManager.GetResponseContent(getRes);
            var data = "email=" + Uri.EscapeDataString(email) + "&password=" + Uri.EscapeDataString(password) + "&fkey=" + CQ.Create(getResContent).GetFkey();

            RequestManager.CookiesToPass = RequestManager.GlobalCookies;
            var res = RequestManager.SendPOSTRequest("https://openid.stackexchange.com/account/login/submit", data);

            if (res == null || !String.IsNullOrEmpty(res.Headers["p3p"]))
            {
                throw new Exception("Could not login using the specified OpenID credentials. Have you entered the correct credentials and have an active internet connection?");
            }
        }

        private void SiteLogin(string host)
        {
            HandleNewAccountPrompt(host);
        }

        private void HandleNewAccountPrompt(string host)
        {
            var em = Uri.EscapeDataString(email);
            var pa = Uri.EscapeDataString(password);
            var na = Uri.EscapeDataString(username);
            var referrer = "https://" + host + "/users/signup?returnurl=http://" + host + "%2f";
            var origin = "https://" + host + ".com";
            var fkey = CQ.Create(RequestManager.GetResponseContent(RequestManager.SendGETRequest("https://" + host + "/users/signup"))).GetFkey();

            var data = "fkey=" + fkey + "&display-name=" + na + "&email=" + em + "&password=" + pa + "&password2=" + pa + "&legalLinksShown=1";

            var postRes = RequestManager.SendPOSTRequest("https://" + host + "/users/signup", data, true, referrer, origin);

            if (postRes == null) { throw new Exception("Could not login/sign-up."); }

            var resContent = RequestManager.GetResponseContent(postRes);

            // We already have an account (and we've been logged in).
            if (!resContent.Contains("We will automatically link this account with your accounts on other Stack Exchange sites.")) { return; }

            // We don't have an account, so lets create one!

            var dom = CQ.Create(resContent);
            var s = dom["input"].First(e => e.Attributes["name"] != null && e.Attributes["name"] == "s").Attributes["value"];

            var signUpRes = RequestManager.SendPOSTRequest("https://" + host + "/users/openidconfirm", "fkey=" + fkey + "&s=" + s + "&legalLinksShown=1", true, referrer, origin);

            if (signUpRes == null) { throw new Exception("Could not login/sign-up."); }
        }
    }
}