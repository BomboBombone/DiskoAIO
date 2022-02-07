using Nethereum.Web3;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using System.Collections.Generic;
using System.Text;
using Nethereum.Signer;
using Nethereum.ABI.Encoders;
using System.Net.Http;
using System.Net;
using System;

namespace DiskoAIO
{
    class DiscordWeb3
    {
        public HttpClient client { get; set; }
        public DiscordToken discordClient { get; set; }
        public HttpClientHandler clientHandler { get; set; }
        public
        Web3 web3 = new Web3("https://mainnet.infura.io/v3/764a45ad934f4e809eb253c83dcb3593");
        public Account account { get; set; }
        public string _url;

        public bool Complete(DiscordToken token, string url)
        {
            try
            {
                InitializeHttpClient();
                discordClient = token;
                _url = url;
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
        public string SendFirstSignature()
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
                RequestUri = new Uri(_url)
            }).GetAwaiter().GetResult();
            return "";
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
