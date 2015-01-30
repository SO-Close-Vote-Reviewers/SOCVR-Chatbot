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
        private readonly string cookieKey;
        private string openidUrl;
        private bool disposed;

        public List<Room> Rooms { get; private set; }



        public Client(string email, string password)
        {
            if (String.IsNullOrEmpty(email) || !email.Contains("@")) { throw new ArgumentException("'email' must be a valid email address.", "email"); }
            if (String.IsNullOrEmpty(password)) { throw new ArgumentException("'password' must not be null or empty.", "password"); }
            
            cookieKey = email.Split('@')[0];
            
            if (RequestManager.Cookies.ContainsKey(cookieKey)) { throw new Exception("Can not create duplicate instances of the same user."); }

            RequestManager.Cookies.Add(cookieKey, new CookieContainer());

            Rooms = new List<Room>();

            SEOpenIDLogin(email, password);
        }

        ~Client()
        {
            Dispose();
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
                    SEChatLogin();
                }
                else
                {
                    SiteLogin(host);
                }
            }

            var r = new Room(cookieKey, host, id);

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
            
            RequestManager.Cookies.Remove(cookieKey);

            GC.SuppressFinalize(this);

            disposed = true;
        }



        private void SEOpenIDLogin(string email, string password)
        {
            var getRes = RequestManager.SendGETRequest(cookieKey, "https://openid.stackexchange.com/account/login");

            if (getRes == null) { throw new Exception("Could not get OpenID fkey. Do you have an active internet connection?"); }

            var getResContent = RequestManager.GetResponseContent(getRes);
            var data = "email=" + Uri.EscapeDataString(email) + "&password=" + Uri.EscapeDataString(password) + "&fkey=" + CQ.Create(getResContent).GetInputValue("fkey");
            var res = RequestManager.SendPOSTRequest(cookieKey, "https://openid.stackexchange.com/account/login/submit", data);

            if (res == null || !String.IsNullOrEmpty(res.Headers["p3p"])) { throw new Exception("Could not login using the specified OpenID credentials. Have you entered the correct credentials and have an active internet connection?"); }

            var postResContent = RequestManager.GetResponseContent(res);
            var dom = CQ.Create(postResContent);

            openidUrl = WebUtility.HtmlDecode(dom["#delegate"][0].InnerHTML);
            openidUrl = openidUrl.Remove(0, openidUrl.LastIndexOf("href", StringComparison.Ordinal) + 6);
            openidUrl = openidUrl.Remove(openidUrl.IndexOf("\"", StringComparison.Ordinal));
        }

        private void SiteLogin(string host)
        {
            var getRes = RequestManager.SendGETRequest(cookieKey, "http://" + host + "/users/login");

            if (getRes == null) { throw new Exception("Could not get fkey from " + host + ". Do you have an active internet connection?"); }

            var getResContent = RequestManager.GetResponseContent(getRes);
            var data = "fkey=" + CQ.Create(getResContent).GetInputValue("fkey") + "&oauth_version=&oauth_server=&openid_username=&openid_identifier=" + Uri.EscapeDataString(openidUrl);
            var postRes = RequestManager.SendPOSTRequest(cookieKey, "http://" + host + "/users/authenticate", data, true, "https://" + host + "/users/login?returnurl=" + Uri.EscapeDataString("http://" + host + "/"));

            if (postRes == null) { throw new Exception("Could not login into site " + host + ". Have you entered the correct credentials and have an active internet connection?"); }

            if (postRes.ResponseUri.ToString().StartsWith("https://openid.stackexchange.com/account/prompt"))
            {
                HandleConfirmationPrompt(postRes);
            }
        }

        private void HandleConfirmationPrompt(HttpWebResponse res)
        {
            if (!res.ResponseUri.ToString().StartsWith("https://openid.stackexchange.com/account/prompt")) { return; }

            var resContent = RequestManager.GetResponseContent(res);
            var dom = CQ.Create(resContent);
            var session = dom["input"].First(e => e.Attributes["name"] != null && e.Attributes["name"] == "session");
            var fkey = dom.GetInputValue("fkey");
            var data = "session=" + session["value"] + "&fkey=" + fkey;

            RequestManager.SendPOSTRequest(cookieKey, "https://openid.stackexchange.com/account/prompt/submit", data);
        }

        private void SEChatLogin()
        {
            // Login to SE.
            RequestManager.SendGETRequest(cookieKey, "http://stackexchange.com/users/authenticate?openid_identifier=" + Uri.EscapeDataString(openidUrl));

            var getRes = RequestManager.SendGETRequest(cookieKey, "http://stackexchange.com/users/chat-login");
            var html = RequestManager.GetResponseContent(getRes);
            var dom = CQ.Create(html);
            var authToken = Uri.EscapeDataString(dom.GetInputValue("authToken"));
            var nonce = Uri.EscapeDataString(dom.GetInputValue("nonce"));
            var data = "authToken=" + authToken + "&nonce=" + nonce;

            // Login to chat.SE.
            var postRes = RequestManager.SendPOSTRequest(cookieKey, "http://chat.stackexchange.com/users/login/global", data, true, "http://chat.stackexchange.com", "http://chat.stackexchange.com");

            if (postRes == null || !RequestManager.GetResponseContent(postRes).StartsWith("{\"Message\":\"Welcome"))
            {
                throw new Exception("Colud not login to (chat) Stack Exchange.");
            }
        }
    }
}
