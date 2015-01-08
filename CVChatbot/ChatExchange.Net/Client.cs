using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using CsQuery;



namespace ChatExchangeDotNet
{
    public class Client : IDisposable
    {
        private readonly Regex hostParser = new Regex("https?://(chat.)?|/.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex idParser = new Regex(".*/rooms/|/.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly string username;
        private readonly string email;
        private readonly string password;
        private bool disposed;

        public List<Room> Rooms { get; private set; }



        public Client(string username, string email, string password)
        {
            if (String.IsNullOrEmpty(email)) { throw new ArgumentException("'email' must not be null or empty.", "email"); }
            if (String.IsNullOrEmpty(password)) { throw new ArgumentException("'password' must not be null or empty.", "password"); }

            this.username = username;
            this.email = email;
            this.password = password;
            Rooms = new List<Room>();

            SEOpenIDLogin(email, password);
        }

        ~Client()
        {
            if (disposed) { return; }

            for (var i = 0; i < Rooms.Count; i++)
            {
                Rooms[i].Dispose();
            }

            disposed = true;
        }



        public Room JoinRoom(string roomUrl)
        {
            var host = hostParser.Replace(roomUrl, "");
            var id = int.Parse(idParser.Replace(roomUrl, ""));

            if (Rooms.Any(room => room.Host == host && room.ID == id)) { throw new Exception("You're already in this room."); }

            if (Rooms.All(room => room.Host != host))
            {
                if (host.ToLowerInvariant() == "stackexchange.com")
                {
                    throw new NotImplementedException();
                }

                SiteLogin(host);
            }

            var r = new Room(host, id);

            Rooms.Add(r);

            return r;
        }

        public void Dispose()
        {
            if (disposed) { return; }

            for (var i = 0; i < Rooms.Count; i++)
            {
                Rooms[i].Dispose();
            }

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

            if (res == null || !String.IsNullOrEmpty(res.Headers["p3p"])) { throw new Exception("Could not login using the specified OpenID credentials. Have you entered the correct credentials and have an active internet connection?"); }
        }

        private void SiteLogin(string host)
        {
            //var getRes = RequestManager.SendGETRequest("http://" + host + "/users/login?returnurl = %%2f");

            //if (getRes == null) { throw new Exception("Could not get fkey from " + host + ". Do you have an active internet connection?"); }

            //var getResContent = RequestManager.GetResponseContent(getRes);

            //var data = "oauth_version=null&oauth_server=null&openid_identifier=" + Uri.EscapeDataString("https://openid.stackexchange.com/") + "&fkey=" + CQ.Create(getResContent).GetFkey();

            //var postRes = RequestManager.SendPOSTRequest("http://" + host + "/users/authenticate", data);

            //if (postRes == null) { throw new Exception("Could not login into site " + host + ". Have you entered the correct credentials and have an active internet connection?"); }

            //if (postRes.ResponseUri.ToString().StartsWith("https://openid.stackexchange.com/account/prompt"))
            //{
            //    HandleConfirmationPrompt(postRes);
            //    return;
            //}

            //if (postRes.ResponseUri.ToString().StartsWith("http://" + host + "/users/authenticate"))
            //{
                HandleNewAccountPrompt(/*postRes, */host);
            //}
        }

        //private void HandleConfirmationPrompt(HttpWebResponse res)
        //{
        //    if (!res.ResponseUri.ToString().StartsWith("https://openid.stackexchange.com/account/prompt")) { return; }

        //    var resContent = RequestManager.GetResponseContent(res);
        //    var dom = CQ.Create(resContent);

        //    var session = dom["input"].First(e => e.Attributes["name"] != null && e.Attributes["name"] == "session");
        //    var fkey = dom.GetFkey();

        //    var data = "session=" + session["value"] + "&fkey=" + fkey;

        //    RequestManager.SendPOSTRequest("https://openid.stackexchange.com/account/prompt/submit", data);
        //}

        private void HandleNewAccountPrompt(/*HttpWebResponse res, */string host)
        {
            //if (!res.ResponseUri.ToString().StartsWith("http://" + host + "/users/authenticate")) { return; }

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
