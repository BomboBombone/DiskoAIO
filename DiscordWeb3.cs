using Nethereum.Web3;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using HtmlAgilityPack;
using System.Text;
using Nethereum.Signer;
using Nethereum.ABI.Encoders;
using System.Net.Http;
using System.Net;
using System;
using Newtonsoft.Json.Linq;
using Discord;
using System.Linq;

namespace DiskoAIO
{
    class DiscordWeb3
    {
        public HttpClient client { get; set; }
        public DiscordClient discordClient { get; set; }
        public HttpClientHandler clientHandler { get; set; }
        public
        Web3 web3 = new Web3("https://mainnet.infura.io/v3/764a45ad934f4e809eb253c83dcb3593");
        public Account account { get; set; }
        public string code { get; set; }

        public DiscordWeb3(DiscordClient token, string url)
        {
            InitializeHttpClient();

            discordClient = token;
            code = url.Split('/')[url.Split('/').Length - 1];
        }
        public bool Complete()
        {
            try
            {
                SendFirstSignature();
                VisitCallBackURL();
                FinalPost();
                return true;
            }
            catch(Exception ex)
            {
                Debug.Log("Exception inside kryptosign module: " + ex.StackTrace);
                return false;
            }
        }
        public string GetFirstSignature()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            account = new Account(privateKey);
            var signer = new EthereumMessageSigner();
            var msg = "Please sign this message to connect to KryptoSign.";
            var signature = signer.EncodeUTF8AndSign(msg, new EthECKey(privateKey));
            return signature;
        }
        public void SendFirstSignature()
        {
            var signature = GetFirstSignature();
            string payload = '{' + $"\"address\":\"{account.Address}\", \"signature\":\"{signature}\"" + '}';
            client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("https://www.kryptosign.io/api/auth/session")
            }).GetAwaiter().GetResult();
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json"),
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.kryptosign.io/api/user/login")
            }).GetAwaiter().GetResult();
        }
        public string GetCSRFToken()
        {
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri("https://www.kryptosign.io/api/auth/csrf")
            }).GetAwaiter().GetResult();
            var json = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            return json.Value<string>("csrfToken");
        }
        public string GetDiscordURL()
        {
            var payload = $"csrfToken={GetCSRFToken()}&callbackUrl=https%3A%2F%2Fwww.kryptosign.io%2Fsign%2F{code}&json=true";
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded"),
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://www.kryptosign.io/api/auth/signin/discord")
            }).GetAwaiter().GetResult();
            var json = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            return json.Value<string>("url");
        }
        public string GetCallBackURL()
        {
            var payload = "{\"permissions\":\"0\",\"authorize\":true}";
            client.DefaultRequestHeaders.Add("Authorization", discordClient.Token);
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json"),
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri(GetDiscordURL())
            }).GetAwaiter().GetResult();
            var json = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            client.DefaultRequestHeaders.Clear();
            return json.Value<string>("location");
        }
        public void VisitCallBackURL()
        {
            client.DefaultRequestHeaders.Add("Referer", "https://discord.com/");
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(GetCallBackURL())
            }).GetAwaiter().GetResult();
        }
        public void FinalPost()
        {
            var doc = new HtmlDocument();
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(GetCallBackURL())
            }).GetAwaiter().GetResult();
            doc.LoadHtml(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            var node = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("pb-4 whitespace-pre-line break-words")).First();

            var msg = node.InnerHtml;
            var signer = new EthereumMessageSigner();
            var signature = signer.EncodeUTF8AndSign(msg, new EthECKey(account.PrivateKey));

            var discriminator = discordClient.User.Discriminator.ToString();
            for(var i = 0; i < 4 - discriminator.Length; i++)
            {
                discriminator = '0' + discriminator;
            }
            var payload = '{' + $"\"friendly_name\":\"{discordClient.User.Id}\", \"crypto_signature\":\"{signature}\", " +
                $"\"signature_type\":\"discord\", \"initial_discord_name\": \"{discordClient.User.Username + '#' + discriminator}\"" + '}';
            response = client.SendAsync(new HttpRequestMessage()
            {
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json"),
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri(GetDiscordURL())
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
