using Discord;
using DiskoAIO.CaptchaSolvers;
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
        public bool _isTwitterConnected { get; set; } = false;
        public bool IsTwitterConnected
        {
            get { return _isTwitterConnected; } 
            set { _isTwitterConnected = value; }
        }
        public bool _isDiscordConnected { get; set; } = false;
        public bool IsDiscordConnected
        {
            get { return _isDiscordConnected; }
            set { _isDiscordConnected = value; }
        }
        public string twitterUsername { get; set; } = "";
        public string discordUserId { get; set; } = "";
        public HttpClient client { get; set; }
        public DiscordClient discordClient { get; set; }
        public HttpClientHandler clientHandler { get; set; }
        public Premint()
        {
            InitializeHttpClient();
        }
        public static Premint Load(List<string> input, bool login = true)
        {
            while(input.Count < 7)
            {
                input.Add("");
            }
            var premint = new Premint();
            premint.Address = input[0];
            premint.private_key = input[1];
            premint._isDiscordConnected = input[2].ToLower() == "true";
            premint._isTwitterConnected = input[3].ToLower() == "true";
            premint.discordUserId = input[4];
            premint.twitterUsername = input[5];
            premint.Note = input.Last();
            if(login)
                premint.Login(new Account(Encoding.ASCII.GetBytes(input[1])));
            return premint;
        }
        public Account Register()
        {
            var mm = DiscordWeb3.GetWallet();

            var payload = new Dictionary<string, string>();
            payload.Add("username", mm.Address);
            //client.DefaultRequestHeaders.Add("X-CSRFToken", Cookies.GetCookieHeader(new Uri("https://www.premint.xyz")).Split('=').Last());
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri("https://www.premint.xyz/v1/signup_api/"),
                Content = new FormUrlEncodedContent(payload)
            }).GetAwaiter().GetResult();
            //client.DefaultRequestHeaders.Remove("X-CSRFToken");
            //client.DefaultRequestHeaders.Add("X-CSRFToken", res.Headers.GetValues("set-cookie").Where(o => o.StartsWith("csrftoken")).First().Split(';').First().Split('=').Last());
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            var nonce = json.Value<string>("data");

            var msg = $"Welcome to PREMINT!\n\nSigning is the only way we can truly know \nthat you are the owner of the wallet you \nare connecting. Signing is a safe, gas-less \ntransaction that does not in any way give \nPREMINT permission to perform any \ntransactions with your wallet.\n\nWallet address:\n{mm.Address}\n\nNonce: " + nonce;
            var signature = DiscordWeb3.SignMessage(msg, mm);
            var dict = new Dictionary<string, string>();
            dict.Add("web3provider", "metamask");
            dict.Add("address", mm.Address.ToLower());
            dict.Add("signature", signature);
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/"),
                Content = new FormUrlEncodedContent(dict)
            }).GetAwaiter().GetResult();
            // client.DefaultRequestHeaders.Remove("X-CSRFToken");
            private_key = mm.PrivateKey;
            Address = mm.Address;
            return mm;
        }
        public void Login(Account mm)
        {
            private_key = mm.PrivateKey;
            Address = mm.Address;
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/")
            }).GetAwaiter().GetResult();
            try
            {
                client.DefaultRequestHeaders.Remove("X-CSRFToken");
            }
            catch (Exception ex) { }
            client.DefaultRequestHeaders.Add("X-CSRFToken", res.Headers.GetValues("set-cookie").Where(o => o.StartsWith("csrftoken")).First().Split(';').First().Split('=').Last());
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            var nonce = json.Value<string>("data");

            var msg = $"Welcome to PREMINT!\n\nSigning is the only way we can truly know \nthat you are the owner of the wallet you \nare connecting. Signing is a safe, gas-less \ntransaction that does not in any way give \nPREMINT permission to perform any \ntransactions with your wallet.\n\nWallet address:\n{mm.Address}\n\nNonce: " + nonce;
            var signature = DiscordWeb3.SignMessage(msg, mm);
            var dict = new Dictionary<string, string>();
            dict.Add("web3provider", "metamask");
            dict.Add("address", mm.Address);
            dict.Add("signature", signature);
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/"),
                Content = new FormUrlEncodedContent(dict)
            }).GetAwaiter().GetResult();
        }
        public void Login()
        {
            var mm = new Account(private_key);
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/")
            }).GetAwaiter().GetResult();
            try
            {
                client.DefaultRequestHeaders.Remove("X-CSRFToken");
                client.DefaultRequestHeaders.Add("X-CSRFToken", res.Headers.GetValues("set-cookie").Where(o => o.StartsWith("csrftoken")).First().Split(';').First().Split('=').Last());

            }
            catch (Exception ex) { }

            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            var nonce = json.Value<string>("data");

            var msg = $"Welcome to PREMINT!\n\nSigning is the only way we can truly know \nthat you are the owner of the wallet you \nare connecting. Signing is a safe, gas-less \ntransaction that does not in any way give \nPREMINT permission to perform any \ntransactions with your wallet.\n\nWallet address:\n{mm.Address.ToLower()}\n\nNonce: " + nonce;
            var signature = DiscordWeb3.SignMessage(msg, mm);
            var dict = new Dictionary<string, string>();
            dict.Add("web3provider", "metamask");
            dict.Add("address", mm.Address.ToLower());
            dict.Add("signature", signature);
            client.DefaultRequestHeaders.Add("Origin", "https://www.premint.xyz");


            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.premint.xyz/v1/login_api/"),
                Content = new FormUrlEncodedContent(dict)
            }).GetAwaiter().GetResult();
            client.DefaultRequestHeaders.Remove("Origin");
        }
        public void ConnectTwitter(string username, string password, string phone = null)
        {
            Login();
            var twitter_client = new Twitter.Twitter(username, password, phone);
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/accounts/twitter/login/?process=connect&next=%2Fdisko-aio-test%2F")
            }).GetAwaiter().GetResult();
            var oauth_token = res.Headers.Location.Query.Split('&').First().Split('=').Last();
            res = twitter_client.client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = res.Headers.Location
            }).GetAwaiter().GetResult();
            var content = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (content.Contains("class=\"happy notice callback\""))
            {
                var next_link = "https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=" + content.Replace("href=\"https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=", "§").Split('§').Last().Split('"').First();
                res = client.SendAsync(new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(next_link)
                }).GetAwaiter().GetResult();
            }
            else
            {
                var auth_token = content.Replace("name=\"authenticity_token\" type=\"hidden\" value=\"", "§").Split('§').Last().Split('"').First();
                //var next_link = "https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=" + content.Replace("name=\"redirect_after_login\" type=\"hidden\" value=\"", "§").Split('§').Last().Split('"').First();
                var payload = new Dictionary<string, string>();
                payload.Add("authenticity_token", auth_token);
                payload.Add("redirect_after_login", "https://api.twitter.com/oauth/authorize?oauth_token=" + oauth_token);
                payload.Add("oauth_token", oauth_token);

                try
                {
                    client.DefaultRequestHeaders.Remove("Referer");
                }
                catch (Exception ex) { }
                twitter_client.client.DefaultRequestHeaders.Remove("Referrer");
                twitter_client.client.DefaultRequestHeaders.Add("Referer", $"https://api.twitter.com/oauth/authenticate?oauth_token={oauth_token}&amp;oauth_callback=https%3A%2F%2Fwww.premint.xyz%2Faccounts%2Ftwitter%2Flogin%2Fcallback%2F");
                //var payload = $"authenticity_token={auth_token}&redirect_after_login=https://api.twitter.com/oauth/authorize?oauth_token={oauth_token}&oauth_token={oauth_token}";
                res = twitter_client.client.SendAsync(new HttpRequestMessage()
                {
                    RequestUri = new Uri("https://api.twitter.com/oauth/authorize"),
                    Method = new HttpMethod("POST"),
                    Content = new FormUrlEncodedContent(payload)
                }).GetAwaiter().GetResult();
                twitter_client.client.DefaultRequestHeaders.Remove("Referrer");
                content = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var next_link = "https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=" + content.Replace("href=\"https://www.premint.xyz/accounts/twitter/login/callback/?oauth_token=", "§").Split('§').Last().Split('"').First();
                res = client.SendAsync(new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(next_link)
                }).GetAwaiter().GetResult();
            }

            if(res.StatusCode != HttpStatusCode.Found || (res.Headers.GetValues("set-cookie").Where(o => o.StartsWith("messages=\"[[\\\"__json_message\\\"\\0540\\05420\\054\\\"The social account has been connected to this wallet.")).Count() == 0))
            {
                throw new Exception("Could not bind twitter account to premint");
            }
            IsTwitterConnected = true;
        }
        public void ConnectDiscord(DiscordToken token)
        {
            Login();
            var res = client.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri("https://www.premint.xyz/accounts/discord/login/?process=connect&next=%2Fdisko-aio-test%2F&scope=guilds.members.read"),
                Method = new HttpMethod("GET")
            }).GetAwaiter().GetResult();
            var state = res.Headers.Location.ToString().Split('=').Last();
            client.DefaultRequestHeaders.Add("Authorization", token._token);
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri("https://discord.com/api/v9/oauth2/authorize?client_id=897003204598980619&response_type=code&redirect_uri=https%3A%2F%2Fwww.premint.xyz%2Faccounts%2Fdiscord%2Flogin%2Fcallback%2F&scope=identify%20email%20guilds%20guilds.members.read&state=" + state),
                Content = new StringContent("{\"permissions\":\"0\",\"authorize\":true}", Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
            client.DefaultRequestHeaders.Remove("Authorization");

            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            var loc = json.Value<string>("location");
            res = client.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri(loc),
                Method = new HttpMethod("GET")
            }).GetAwaiter().GetResult();
            foreach(var cookie in res.Headers.GetValues("set-cookie"))
            {
                if (cookie.StartsWith("messages=\"[[\\\"__json_message\\\"\\0540\\05420\\054\\\"The social account has been connected to this wallet."))
                {
                    IsDiscordConnected = true;
                }
            }
        }
        public void SubscribeToProject(string project_name, bool solve_captcha = false)
        {
            string url = "https://www.premint.xyz/" + project_name + '/';
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(url)
            }).GetAwaiter().GetResult();
            client.DefaultRequestHeaders.Add("X-CSRFToken", res.Headers.GetValues("set-cookie").Where(o => o.StartsWith("csrftoken")).First().Split(';').First().Split('=').Last());

            var content = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var csrf_token = content.Replace("<input type=\"hidden\" name=\"csrfmiddlewaretoken\" value=\"", "§").Split('§').Last().Split('"').First();
            var dict = new Dictionary<string, string>();
            dict.Add("csrfmiddlewaretoken", csrf_token);
            dict.Add("params_field", "{}");
            dict.Add("email_field", "");
            if (solve_captcha)
            {
                dict.Add("g-recaptcha-response", PremintSolver.Solve(project_name));
            }
            dict.Add("registration-form-submit", "");
            var payload = new FormUrlEncodedContent(dict);
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri(url),
                Content = payload
            }).GetAwaiter().GetResult();
            if(res.StatusCode != HttpStatusCode.Found)
            {
                throw new Exception("Could not join project premint");
            }
        }
        public void InitializeHttpClient()
        {
            clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            client = new System.Net.Http.HttpClient(clientHandler);
            client.DefaultRequestHeaders.Add("Referer", "https://www.premint.xyz");
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.premint.xyz/")
            }).GetAwaiter().GetResult();
            //client.DefaultRequestHeaders.Add("X-CSRFToken", res.Headers.GetValues("set-cookie").Where(o => o.StartsWith("csrftoken")).First().Split(';').First().Split('=').Last());

        }

        public CookieContainer Cookies
        {
            get { return clientHandler.CookieContainer; }
            set { clientHandler.CookieContainer = value; }
        }
        public override string ToString()
        {
            return $"{Address}:{private_key}:{IsDiscordConnected}:{IsTwitterConnected}:{discordUserId}:{twitterUsername}:{Note}";
        }
    }
}
