using Discord;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO.Premint
{
    public class Premint
    {
        public string _address { get; set; }
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }
        public string _note { get; set; }
        public string Note
        {
            get
            {
                if (_note == null)
                    _note = "Double click to add note...";
                return _note;
            }
            set { _note = value; }
        }
        public string private_key { get; set; }
        public HttpClient client { get; set; }
        public DiscordClient discordClient { get; set; }
        public HttpClientHandler clientHandler { get; set; }
        public Premint()
        {
            InitializeHttpClient();
        }
        public string Register()
        {
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            var nonce = json.Value<string>("data");
            var mm = DiscordWeb3.GetWallet();

            var msg = $"Welcome to PREMINT!\n\nSigning is the only way we can truly know \nthat you are the owner of the wallet you \nare connecting. Signing is a safe, gas-less \ntransaction that does not in any way give \nPREMINT permission to perform any \ntransactions with your wallet.\n\nWallet address:\n{mm.Address}\n\nNonce: " + nonce;
            var signature = DiscordWeb3.SignMessage(msg, mm);
            var payload = $"web3provider=metamask&address={mm.Address}&signature={signature}";
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "x-www-form-urlencoded")
            }).GetAwaiter().GetResult();
            return mm.PrivateKey;
        }
        public void Login(Account mm)
        {
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            var nonce = json.Value<string>("data");

            var msg = $"Welcome to PREMINT!\n\nSigning is the only way we can truly know \nthat you are the owner of the wallet you \nare connecting. Signing is a safe, gas-less \ntransaction that does not in any way give \nPREMINT permission to perform any \ntransactions with your wallet.\n\nWallet address:\n{mm.Address}\n\nNonce: " + nonce;
            var signature = DiscordWeb3.SignMessage(msg, mm);
            var payload = $"web3provider=metamask&address={mm.Address}&signature=0x{signature}";
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "x-www-form-urlencoded")
            }).GetAwaiter().GetResult();
        }
        public void ConnectTwitter(string username, string password, string phone = null)
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
            var content = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //var auth_token = content.Replace("<input type=\"hidden\" name=\"authenticity_token\" value=\"", "§").Split('§').Last().Split('"').First();
            var next_link = "href=\"https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=" + content.Replace("href=\"https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=", "§").Split('§').Last().Split('"').First();
            client.DefaultRequestHeaders.Add("Referer", "https://api.twitter.com/oauth/authenticate?oauth_token=9U-QAgAAAAABUjydAAABf2BoDwc&oauth_callback=https%3A%2F%2Fwww.premint.xyz%2Faccounts%2Ftwitter%2Flogin%2Fcallback%2F");
            //var payload = $"authenticity_token={auth_token}&redirect_after_login=https://api.twitter.com/oauth/authorize?oauth_token={oauth_token}&oauth_token={oauth_token}";
            res = twitter_client.client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(next_link)
            }).GetAwaiter().GetResult();
            if(res.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Could not bind twitter account to premint");
            }
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
