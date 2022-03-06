using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO.Premint
{
    class Premint
    {
        public HttpClient client { get; set; }
        public DiscordClient discordClient { get; set; }
        public HttpClientHandler clientHandler { get; set; }
        public Premint()
        {
            InitializeHttpClient();
        }
        public void Connect(string username, string password, string phone = null)
        {
            var twitter_client = new Twitter.Twitter(username, password, phone);
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/accounts/twitter/login/?process=connect")
            }).GetAwaiter().GetResult();
            var oauth_token = res.Headers.Location.Query.Split('&').First().Split('=').Last();
            res = twitter_client.client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = res.Headers.Location
            }).GetAwaiter().GetResult();
            var auth_token = res.Content.ReadAsStringAsync().GetAwaiter().GetResult().Replace("<input type=\"hidden\" name=\"authenticity_token\" value=\"", "§").Split('§').Last().Split('"').First();
            client.DefaultRequestHeaders.Add("Referer", "https://api.twitter.com/oauth/authenticate?oauth_token=9U-QAgAAAAABUjydAAABf2BoDwc&oauth_callback=https%3A%2F%2Fwww.premint.xyz%2Faccounts%2Ftwitter%2Flogin%2Fcallback%2F");
            var payload = $"authenticity_token={auth_token}&redirect_after_login=https://api.twitter.com/oauth/authorize?oauth_token={oauth_token}&oauth_token={oauth_token}";
            res = twitter_client.client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri("https://api.twitter.com/oauth/authorize"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "x-www-form-urlencoded")
            }).GetAwaiter().GetResult();
        }
        public void InitializeHttpClient()
        {
            clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            client = new System.Net.Http.HttpClient(clientHandler);
        }

        public CookieContainer Cookies
        {
            get { return clientHandler.CookieContainer; }
            set { clientHandler.CookieContainer = value; }
        }
    }
}
