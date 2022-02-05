using Newtonsoft.Json.Linq;
using System.Threading;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using Leaf.xNet;
using System.Net;
using System.Threading.Tasks;
using DiskoAIO;
using System.Security.Authentication;
using System.IO;

namespace Discord
{
    public class DiscordHttpClient
    {
        private readonly DiscordClient _discordClient;
        public string BaseUrl => DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel);
        public string _fingerprint { get; set; }
        public DiscordHttpClient(DiscordClient discordClient)
        {
            _discordClient = discordClient;
            /*
            if (!(Settings.Default.Fingerprint != null && Settings.Default.Fingerprint != ""))
                Settings.Default.Fingerprint = GetFingerprint().GetAwaiter().GetResult();
            _fingerprint = Settings.Default.Fingerprint;
            */
        }
        public async Task<string> GetFingerprint()
        {
            HttpClient client = new HttpClient();
            var response_fingerprint = await client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri("https://discord.com/api/v9/experiments")
            });
            var resp_fingerprint = new DiscordHttpResponse((int)response_fingerprint.StatusCode, response_fingerprint.Content.ReadAsStringAsync().Result);
            var fingerprint = resp_fingerprint.Body.Value<String>("fingerprint");

            return fingerprint;
        }

        /// <summary>
        /// Sends an HTTP request and checks for errors
        /// </summary>
        /// <param name="method">HTTP method to use</param>
        /// <param name="endpoint">API endpoint (fx. /users/@me)</param>
        /// <param name="payload">JSON content</param>

        private async Task<DiscordHttpResponse> SendAsync(Leaf.xNet.HttpMethod method, string endpoint, object payload = null, ulong? guildId = null, ulong? channelId = null)
        {
            if (!endpoint.StartsWith("https"))
                endpoint = DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel) + endpoint;

            string json = "{}";
            if (payload != null)
            {
                if (payload.GetType() == typeof(string))
                    json = (string)payload;
                else
                    json = JsonConvert.SerializeObject(payload);
            }

            uint retriesLeft = _discordClient.Config.RestConnectionRetries;
            bool hasData = method == Leaf.xNet.HttpMethod.POST || method == Leaf.xNet.HttpMethod.PATCH || method == Leaf.xNet.HttpMethod.PUT || method == Leaf.xNet.HttpMethod.DELETE;

            while (true)
            {
                try
                {
                    DiscordHttpResponse resp = null;

                    if (method == Leaf.xNet.HttpMethod.POST && (endpoint.Contains("/invites/") || endpoint == "https://discord.com/api/v9/users/@me/channels"))
                    {
                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                            //SslProtocols = SslProtocols.Tls12
                        };
                        request.Proxy = _discordClient.Proxy;
                        /*
                        request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port);
                        if (_discordClient.Proxy.Username != null && _discordClient.Proxy.Username != "")
                            request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port, _discordClient.Proxy.Username, _discordClient.Proxy.Password);
                        */
                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "it");
                        request.AddHeader("Authorization", _discordClient.Token);
                        request.AddHeader("Connection", "keep-alive");
                        request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
                        request.AddHeader("DNT", "1");
                        request.AddHeader("origin", "https://discord.com");
                        request.AddHeader("Referer", "https://discord.com/channels/@me");
                        request.AddHeader("TE", "Trailers");
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());

                        HttpResponse response;
                        if(endpoint == "https://discord.com/api/v9/users/@me/channels")
                        {
                            request.AddHeader("x-context-properties", "e30=");
                        }
                        else
                        {
                            var context_pr = "{" + $"\"location\":\"Join Guild\",\"location_guild_id\":\"{guildId}\",\"location_channel_id\":\"{channelId}\",\"location_channel_type\":0" + "}";
                            var encoded_pr = Base64Encode(context_pr);
                            request.AddHeader("X-Context-Properties", encoded_pr);
                        }
                        request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
                        response = request.Post(endpoint, json, "application/json");
                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    else if(method == Leaf.xNet.HttpMethod.PATCH && endpoint == "https://discord.com/api/v9/users/@me")
                    {
                        if (!json.Contains("avatar"))
                        {
                            var json_arr = json.Split(',');
                            json = json_arr[0] + "," + json_arr[3] + "}";
                        }
                        else
                        {
                            var json_arr = json.Split(',');
                            json = "{" + json_arr[3] + ',' + json_arr[4] + "}";
                        }
                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                            //SslProtocols = SslProtocols.Tls12
                        };
                        request.Proxy = _discordClient.Proxy;
                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "it");
                        request.AddHeader("Authorization", _discordClient.Token);
                        request.AddHeader("Connection", "keep-alive");
                        request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
                        request.AddHeader("DNT", "1");
                        request.AddHeader("origin", "https://discord.com");
                        request.AddHeader("Referer", "https://discord.com/channels/@me");
                        request.AddHeader("TE", "Trailers");
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
                        request.AddHeader("X-Debug-Options", "bugReporterEnabled");
                        if (Fingerprint.fingerprint == null)
                            Fingerprint.GetFingerprint().GetAwaiter().GetResult();
                        request.AddHeader("X-Fingerprint", Fingerprint.fingerprint);
                        
                        var response = request.Patch(endpoint, json, "application/json");

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    else if (_discordClient.Proxy == null || _discordClient.Proxy.Type == ProxyType.HTTP)
                    {
                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                            //SslProtocols = SslProtocols.Tls12
                        };
                        request.Proxy = _discordClient.Proxy;
                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "it");
                        var token = _discordClient.Token == null ? null : _discordClient.Token;

                        if (_discordClient.Token != null)
                            request.AddHeader("Authorization", _discordClient.Token);
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("Accept-Language", "it");
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        var jsonContent = new Leaf.xNet.StringContent(json)
                        {
                            ContentType = "application/json"
                        };
                        HttpResponse response = null;
                        try
                        {
                            response = request.Raw(method, endpoint, jsonContent);
                            if (response.StatusCode == Leaf.xNet.HttpStatusCode.BadRequest)
                                throw new Exception();
                            resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                        }
                        catch(Exception ex)
                        {
                            Debug.Log(ex.StackTrace);
                            if(response != null)
                            {
                                Debug.Log("Request: " + ex.StackTrace);
                            }
                        }
                    }
                    else
                    {
                        HttpRequest msg = new HttpRequest
                        {
                            IgnoreProtocolErrors = true,
                            UserAgent = _discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot ? "Anarchy/0.8.1.0" : _discordClient.Config.SuperProperties.UserAgent,
                            Authorization = _discordClient.Token
                        };

                        if (hasData)
                            msg.AddHeader(HttpHeader.ContentType, "application/json");

                        if (_discordClient.User == null || _discordClient.User.Type == DiscordUserType.User) msg.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        if (_discordClient.Proxy != null) msg.Proxy = _discordClient.Proxy;

                        if (endpoint.Contains("https://discord.com/api/v9/channels/") && endpoint.Contains("messages") && method == Leaf.xNet.HttpMethod.POST)
                        {
                            var ep = endpoint.Split('/');
                            ulong guild_id = 0;
                            foreach (string chunk in ep)
                            {
                                if (ulong.TryParse(chunk, out guild_id))
                                {
                                    break;
                                }
                            }
                            msg.AddHeader("origin", "https://discord.com");
                            msg.AddHeader("Accept", "*/*");
                            msg.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d; locale=it");
                            msg.AddHeader(HttpHeader.Referer, "https://discord.com/channels/@me/" + guild_id);
                        }

                        var response = msg.Raw(method, endpoint, hasData ? new Leaf.xNet.StringContent(json) : null);
                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    if (endpoint.Contains("member-verification?") && method == Leaf.xNet.HttpMethod.GET)
                    {
                        try
                        {
                            var ep = endpoint.Split('/');
                            ulong guild_id = 0;
                            foreach (string chunk in ep)
                            {
                                if (ulong.TryParse(chunk, out guild_id))
                                {
                                    break;
                                }
                            }
                            endpoint = $"/guilds/{guild_id}/requests/@me";
                            if (!endpoint.StartsWith("https"))
                                endpoint = DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel) + endpoint;

                            HttpRequest request = new HttpRequest()
                            {
                                KeepTemporaryHeadersOnRedirect = false,
                                EnableMiddleHeaders = false,
                                AllowEmptyHeaderValues = false
                            };

                            request.Proxy = _discordClient.Proxy;

                            var form = JsonConvert.DeserializeObject<GuildVerificationForm>(JsonConvert.SerializeObject(resp.Body));
                            int time_zone = 0;
                            Debug.Log("Fetched time zone for TOS: " + form.Version);
                            try
                            {
                                time_zone = int.Parse(form.Version.Split('+')[1].Split(':')[0]);

                            }
                            catch(Exception ex)
                            {
                                time_zone = int.Parse(form.Version.Split('-')[1].Split(':')[0]);
                            }
                            StringBuilder sb = new StringBuilder(form.Version);
                            int h = int.Parse(sb[11].ToString() + sb[12].ToString());
                            h = h - time_zone;
                            if (h < 1)
                                h = 24 - h;
                            //sb.Replace("+02:00", "000+00:00");
                            //form.Version = sb.ToString();
                            var hString = h.ToString();
                            if (hString.Length < 2)
                            {
                                while (hString.Length < 2)
                                {
                                    hString = "0" + hString;
                                }
                            }
                            var time_zone_string = time_zone.ToString();
                            if (time_zone_string.Length < 2)
                            {
                                while (time_zone_string.Length < 2)
                                {
                                    time_zone_string = "0" + time_zone_string;
                                }
                            }
                            string buf1 = form.Version.Substring(0, 11) + hString + form.Version.Substring(13);
                            form.Version = buf1.Replace("+" + time_zone_string + ":00", "000+00:00");
                            string json_string = "";
                            try
                            {
                                form.Fields[0].Response = true;
                            }
                            catch { }
                            form.Description = null;
                            var desc = "{\"description\":null,";
                            json_string = JsonConvert.SerializeObject(form, Formatting.None);
                            if (!resp.Body.ToString().Contains(",\"description\":null,\"automations\":null"))
                            {
                                if (resp.Body.ToString().Contains("\"description\":null"))
                                {
                                    json_string = json_string.Replace(",\"automations\":null", "");
                                }
                                else
                                {
                                    Debug.Log("Got TOS");
                                    //json_string = json_string.Replace(",\"description\":null,\"automations\":null", "");
                                }
                            }
                            json_string = "{" + json_string.Substring(desc.Length);
                            request.ClearAllHeaders();
                            request.AddHeader("Accept", "*/*");
                            request.AddHeader("Accept-Encoding", "gzip, deflate");
                            request.AddHeader("Accept-Language", "it");
                            request.AddHeader("Authorization", _discordClient.Token);
                            //request.AddHeader("Content-Type", "application/json");
                            request.AddHeader("Content-Length", (ASCIIEncoding.UTF8.GetBytes(json_string).Length).ToString());
                            request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d; __stripe_mid=bb9db4c2-e791-41a3-aa16-2cbff76990bc770029");
                            request.AddHeader("origin", "https://discord.com");
                            request.AddHeader("Referer", $"https://discord.com/channels/{guild_id}");
                            request.AddHeader("TE", "Trailers");
                            request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                            request.AddHeader("X-Debug-Options", "bugReporterEnabled");
                            //request.AddHeader("x-fingerprint", "903995807798296608.8ycsY24VnE7UWwSqpXx2yeE-AfM");
                            request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                            request.AddHeader("x-discord-locale", "en-US");
                            var response = request.Put(endpoint, json_string, "application/json");
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("Error during TOS acceptance: " + ex.StackTrace);
                        }

                        //var resp1 = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }

                    DiscordHttpUtil.ValidateResponse(resp.StatusCode, resp.Body);
                    return resp;
                }
                catch (Exception ex) when (ex is HttpException || ex is HttpRequestException || ex is TaskCanceledException)
                {
                    Debug.Log(ex.StackTrace);
                    if (ex.Message.Contains("403"))
                        throw new DiscordHttpException(new DiscordHttpError());
                    if (ex.Message.EndsWith("429"))
                        throw;
                    if (ex.Message.EndsWith("410"))
                    {
                        Debug.Log("User has already verified rules");
                    }
                    if (retriesLeft == 0)
                        throw new DiscordConnectionException();

                    retriesLeft--;
                }
                catch (RateLimitException ex)
                {
                    Debug.Log("Rate limited");

                    if (_discordClient.Config.RetryOnRateLimit)
                        Thread.Sleep(ex.RetryAfter);
                    else
                        throw new DiscordHttpException(new DiscordHttpError(DiscordError.CannotExecuteInDM, "Rate Limit"));
                }
            }
        }

        private async Task<DiscordHttpResponse> SendAsyncJoin(Leaf.xNet.HttpMethod method, string endpoint, object payload = null)
        {
            if (!endpoint.StartsWith("https"))
                endpoint = DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel) + endpoint;
            string inv_code = "https://discord.com/api/v9/invites/";
            if (endpoint.Contains("/invites/"))
                inv_code = endpoint.Substring(inv_code.Length);

            string json = "{}";
            if (payload != null)
            {
                if (payload.GetType() == typeof(string))
                    json = (string)payload;
                else
                    json = JsonConvert.SerializeObject(payload);
            }

            uint retriesLeft = _discordClient.Config.RestConnectionRetries;
            bool hasData = method == Leaf.xNet.HttpMethod.POST || method == Leaf.xNet.HttpMethod.PATCH || method == Leaf.xNet.HttpMethod.PUT || method == Leaf.xNet.HttpMethod.DELETE;

            while (true)
            {
                try
                {
                    DiscordHttpResponse resp;

                    if (_discordClient.Proxy == null || _discordClient.Proxy.Type == ProxyType.HTTP)
                    {
                        HttpClient client = new HttpClient(new HttpClientHandler() { Proxy = _discordClient.Proxy == null ? null : new WebProxy(_discordClient.Proxy.Host, _discordClient.Proxy.Port) });
                        var context = ContextProperties.GetContextProperties(inv_code).GetAwaiter().GetResult();

                        if (_discordClient.Token != null)
                            client.DefaultRequestHeaders.Add("authorization", _discordClient.Token);
                        if (_discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot)
                            client.DefaultRequestHeaders.Add("User-Agent", "Anarchy/0.8.1.0");
                        else
                        {
                            client.DefaultRequestHeaders.Add("accept", "*/*");
                            client.DefaultRequestHeaders.Add("accept-language", "it");
                            client.DefaultRequestHeaders.Add("origin", "https://discord.com");
                            client.DefaultRequestHeaders.Add("referer", "https://discord.com/channels/@me");
                            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                            client.DefaultRequestHeaders.Add("sec-gpc", "1");
                            client.DefaultRequestHeaders.Add("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                            client.DefaultRequestHeaders.Add("X-Context-Properties", context);
                            client.DefaultRequestHeaders.Add("X-Debug-Options", "bugReporterEnabled");
                            client.DefaultRequestHeaders.Add("X-Fingerprint", _fingerprint);
                            client.DefaultRequestHeaders.Add("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        }

                        var response = await client.SendAsync(new HttpRequestMessage()
                        {
                            Content = hasData ? new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json") : null,
                            Method = new System.Net.Http.HttpMethod(method.ToString()),
                            RequestUri = new Uri(endpoint)
                        });
                        client.DefaultRequestHeaders.Clear();

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        HttpRequest msg = new HttpRequest
                        {
                            IgnoreProtocolErrors = true,
                            UserAgent = _discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot ? "Anarchy/0.8.1.0" : _discordClient.Config.SuperProperties.UserAgent,
                            Authorization = _discordClient.Token
                        };

                        if (hasData)
                            msg.AddHeader(HttpHeader.ContentType, "application/json");

                        if (_discordClient.User == null || _discordClient.User.Type == DiscordUserType.User) msg.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        if (_discordClient.Proxy != null) msg.Proxy = _discordClient.Proxy;

                        var response = msg.Raw(method, endpoint, hasData ? new Leaf.xNet.StringContent(json) : null);

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }

                    DiscordHttpUtil.ValidateResponse(resp.StatusCode, resp.Body);
                    return resp;
                }
                catch (Exception ex) when (ex is HttpException || ex is HttpRequestException || ex is TaskCanceledException)
                {
                    if (retriesLeft == 0)
                        throw new DiscordConnectionException();

                    retriesLeft--;
                }
                catch (RateLimitException ex)
                {
                    if (_discordClient.Config.RetryOnRateLimit)
                        Thread.Sleep(ex.RetryAfter);
                    else
                        throw;
                }
            }
        }


        public async Task<DiscordHttpResponse> GetAsync(string endpoint)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.GET, endpoint);
        }


        public async Task<DiscordHttpResponse> PostAsync(string endpoint, object payload = null, ulong? guildId = null, ulong? channelId = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.POST, endpoint, payload, guildId, channelId);
        }

        public async Task<DiscordHttpResponse> PostAsyncJoin(string endpoint, object payload = null)
        {
            return await SendAsyncJoin(Leaf.xNet.HttpMethod.POST, endpoint, payload);
        }

        public async Task<DiscordHttpResponse> DeleteAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.DELETE, endpoint, payload);
        }

        public async Task<DiscordHttpResponse> PutAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.PUT, endpoint, payload);
        }


        public async Task<DiscordHttpResponse> PatchAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.PATCH, endpoint, payload);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

    public static class Fingerprint
    {
        [JsonProperty("assignments")]
        public static Array assignments { get; set; } = null;
        [JsonProperty("fingerprint")]
        public static string fingerprint { get; set; } = null;
        public static async Task GetFingerprint()
        {
            string request_url = "https://discord.com/api/v9/experiments";
            HttpClient client = new HttpClient();
            var response_context = await client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(request_url)
            });
            var resp_context = new DiscordHttpResponse((int)response_context.StatusCode, response_context.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(resp_context.Body.ToString());
            Fingerprint.fingerprint = json.Value<string>("fingerprint");
        }
    }
}