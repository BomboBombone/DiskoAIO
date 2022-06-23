using Discord;
using Discord.Gateway;
using DiskoAIO.CaptchaSolvers;
using DiskoAIO.Properties;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    class JoinTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.Join; }
            set { }
        }
        public string Type
        {
            get { return "Join"; }
        }
        private Progress _progress;
        public Progress progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
            }
        }
        private AccountGroup _accountGroup;
        public AccountGroup accountGroup
        {
            get
            {
                return _accountGroup;
            }
            set {
                _accountGroup = value;
            }
        }
        public string accountGroupName
        {
            get
            {
                return _accountGroup._name;
            }
            set
            {
                _accountGroup._name = value;
            }
        }
        private ProxyGroup _proxyGroup;
        public ProxyGroup proxyGroup
        {
            get { return _proxyGroup; }
            set { _proxyGroup = value; }
        }
        public string Account
        {
            get { return _accountGroup._name; }
        }
        public string Proxy
        {
            get {
                if (_proxyGroup != null)
                    return _proxyGroup._name;
                else
                    return "None";
            }
        }
        public bool Running
        {
            get { return joining && !paused; }
            set { joining = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }
        public string invite { get; set; }

        public int delay { get; set; } = 2;
        public int max_tokens { get; set; } = 0;
        ulong serverID { get; set; }
        ulong channelID { get; set; }
        int skip { get; set; }
        public bool joining { get; set; } = true;
        public bool paused { get; set; } = false;

        public bool acceptRules { get; set; }
        public bool bypassReaction { get; set; }
        public bool bypassCaptcha { get; set; }
        public ulong captchaChannelID { get; set; }
        public string captchaBotType { get; set; }

        public JoinTask(AccountGroup accounts, string _invite, ulong channel_id, ProxyGroup proxies = null, int _delay = 2, int max = 0, int skip_tokens = 0, bool rules = false, bool reaction = false, bool captcha = false, ulong captchaChannelId = 0, string captchaType = "Wick")
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            delay = _delay * 1000;
            if (max == 0)
                max = accounts._accounts.Count;
            max_tokens = max;
            channelID = channel_id;
            skip = skip_tokens;
            invite = _invite;
            progress = new Progress(max);

            acceptRules = rules;
            bypassReaction = reaction;
            bypassCaptcha = captcha;
            captchaChannelID = captchaChannelId;
            captchaBotType = captchaType;
        }
        public void Start()
        {
            try
            {
                if (invite.StartsWith("https://discord.gg/"))
                    invite = invite.Remove(0, "https://discord.gg/".Length);
                if (invite.StartsWith("discord.gg/"))
                    invite = invite.Remove(0, "discord.gg/".Length);
                if (invite.StartsWith("https://discord.com/invite/"))
                    invite = invite.Remove(0, "https://discord.com/invite/".Length);
                var (guildId, channelWelcomeId) = Get_GuildID(invite, proxyGroup);
                if (guildId == "1" || guildId == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.mainWindow.ShowNotification("IP banned from Discord, try again using proxies or a VPN");
                    });
                    joining = false;
                    return;
                }
                serverID = ulong.Parse(guildId);
                var clients = new List<DiscordClient>() { };
                var joined = 0;
                var token_list = new List<string>() { };
                foreach (var tk in accountGroup._accounts)
                {
                    if (tk == null)
                        continue;
                    if (accountGroup._accounts.IndexOf(tk) < skip)
                    {
                        _progress.Add(1);
                        continue;
                    }
                    if (token_list.Count == max_tokens)
                        break;
                    token_list.Add(tk._token);
                }
                Thread joiner = new Thread(() =>
                {
                    try
                    {
                        int verifyingCount = 0;
                        var joined_time = DateTime.Now;
                        var messages = new List<DiscordMessage>();
                        var captchaMessages = new List<DiscordMessage>();

                        DiscordMessage mes = null;
                        MessageReaction reaction = null;
                        while (joining)
                        {
                            while (paused)
                                Thread.Sleep(500);
                            try
                            {
                                if (joined == max_tokens)
                                    joining = false;
                            }
                            catch (Exception ex) { }
                            int i = 0;
                            try
                            {
                                var clients1 = new DiscordClient[clients.Count + 10];
                                clients.CopyTo(clients1);
                                foreach (var client in clients1)
                                {
                                    if (!joining)
                                        break;
                                    if (client == null)
                                        continue;
                                    while (paused)
                                        Thread.Sleep(500);
                                    try
                                    {
                                        bool hasJoined = false;
                                        int c = 0;
                                        while (!hasJoined && c == 0)
                                        {
                                            try
                                            {
                                                joined_time = DateTime.Now;

                                                if(bypassCaptcha && captchaBotType == "Discord")
                                                {
                                                    var captcha_data = client.AttemptJoinAsync(invite).GetAwaiter().GetResult();
                                                    if (captcha_data.captcha_sitekey == null)
                                                        break;
                                                    var hcaptcha_key = DiscordSolver.Solve(captcha_data.captcha_sitekey);
                                                    client.JoinGuild(invite, hcaptcha_key);
                                                }
                                                else
                                                    client.JoinGuild(invite);
                                                Task.Run(() =>
                                                {
                                                    verifyingCount++;
                                                    if (acceptRules)
                                                    {
                                                        var z = 0;
                                                        while (z < 1)
                                                        {
                                                            try
                                                            {
                                                                var accepted = client.GetGuildVerificationForm(serverID, invite);
                                                                break;
                                                            }
                                                            catch (Exception ex) { z++; Debug.Log("Couldn't accept TOS - " + ex.StackTrace); }
                                                        }
                                                    }
                                                    if (bypassReaction)
                                                    {
                                                        if (messages.Count == 0 || mes == null)
                                                        {
                                                            try
                                                            {
                                                                messages = (List<DiscordMessage>)client.GetChannelMessages(channelID, new MessageFilters()
                                                                {
                                                                    Limit = 50
                                                                });
                                                                foreach (var message in messages)
                                                                {
                                                                    reaction = null;
                                                                    var reactions = message.Reactions;
                                                                    foreach (var react in reactions)
                                                                    {
                                                                        if (reaction == null)
                                                                            reaction = react;
                                                                        else
                                                                        {
                                                                            if (react.Count > reaction.Count)
                                                                                reaction = react;
                                                                        }

                                                                    }
                                                                    try
                                                                    {
                                                                        if(reaction.ClientHasReacted)
                                                                            client.RemoveMessageReaction(channelID, message.Id, reaction.Emoji.Name, reaction.Emoji.Id);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {

                                                                    }
                                                                    try
                                                                    {
                                                                        client.AddMessageReaction(channelID, message.Id, reaction.Emoji.Name, reaction.Emoji.Id);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Debug.Log(ex.StackTrace);
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Debug.Log(ex.StackTrace);
                                                            }
                                                        }
                                                    }
                                                    if (bypassCaptcha)
                                                    {
                                                        try
                                                        {
                                                            if (captchaBotType == "Wick")
                                                            {
                                                                client.Proxy = null;
                                                                if (captchaMessages.Count == 0 || mes == null)
                                                                {
                                                                    captchaMessages = (List<DiscordMessage>)client.GetChannelMessages(captchaChannelID, new MessageFilters()
                                                                    {
                                                                        Limit = 50
                                                                    });
                                                                }
                                                                DateTime clicked = DateTime.Now;
                                                                foreach (var message in captchaMessages)
                                                                {
                                                                    if (message.Components.Count < 1 || message.Author.User.Id != 548410451818708993)
                                                                        continue;
                                                                    try
                                                                    {
                                                                        Click_Button(client, message);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Debug.Log(ex.Message);
                                                                    }
                                                                }
                                                                var dm = client.CreateDM(548410451818708993);
                                                                var dms = new List<DiscordMessage>();
                                                                int tries = 0;
                                                                while (true && tries < 20)
                                                                {
                                                                    try
                                                                    {
                                                                        dms = (List<DiscordMessage>)dm.GetMessages(new MessageFilters()
                                                                        {
                                                                            Limit = 50
                                                                        });
                                                                        tries++;
                                                                        if (dms.Count < 1)
                                                                            continue;
                                                                        else
                                                                        {
                                                                            if (dms.First().SentAt.Subtract(clicked).TotalSeconds >= 0)
                                                                                break;
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Thread.Sleep(1000);
                                                                        tries++;

                                                                    }
                                                                }
                                                                if (tries == 20)
                                                                    throw new Exception("Token seems to be already verified");
                                                                string solution = "";
                                                                foreach (var message in dms)
                                                                {
                                                                    if (message.Embed.Image != null)
                                                                    {
                                                                        solution = CaptchaSolvers.WickSolver.Solve(message.Embed.Image.Url);
                                                                        while (solution == null)
                                                                            solution = CaptchaSolvers.WickSolver.Solve(message.Embed.Image.Url);

                                                                        break;
                                                                    }
                                                                }
                                                                dm.SendMessage(solution, false);
                                                                tries = 0;
                                                                while (true && tries < 3)
                                                                {
                                                                    try
                                                                    {
                                                                        dms = (List<DiscordMessage>)dm.GetMessages(new MessageFilters()
                                                                        {
                                                                            Limit = 50
                                                                        });
                                                                        tries++;
                                                                        if (dms.Count < 1)
                                                                            continue;
                                                                        else
                                                                        {
                                                                            if (dms.First().Embed != null && dms.First().Embed.Title.Contains("You have been verified!"))
                                                                            {
                                                                                break;
                                                                            }
                                                                            else
                                                                            {
                                                                                foreach (var dm1 in dms)
                                                                                {
                                                                                    try
                                                                                    {
                                                                                        if (dm1.Embed.Image != null)
                                                                                        {
                                                                                            solution = CaptchaSolvers.WickSolver.Solve(dm1.Embed.Image.Url);
                                                                                            if (solution == null)
                                                                                                continue;
                                                                                            dm.SendMessage(solution, false);

                                                                                            break;
                                                                                        }
                                                                                    }
                                                                                    catch (Exception ex) { }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Thread.Sleep(1000);
                                                                        tries++;
                                                                    }
                                                                }
                                                            }
                                                            else if(captchaBotType == "Captcha.bot")
                                                            {
                                                                client.Proxy = null;
                                                                if (captchaMessages.Count == 0 || mes == null)
                                                                {
                                                                    captchaMessages = (List<DiscordMessage>)client.GetChannelMessages(captchaChannelID, new MessageFilters()
                                                                    {
                                                                        Limit = 50
                                                                    });
                                                                }
                                                                DateTime clicked = DateTime.Now;
                                                                string link = null;
                                                                foreach (var message in captchaMessages)
                                                                {
                                                                    if (message.Components.Count < 1 || message.Author.User.Id != 512333785338216465)
                                                                        continue;
                                                                    try
                                                                    {
                                                                        link = Click_Button_CaptchaBot(client, message);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        Debug.Log(ex.Message);
                                                                    }
                                                                }
                                                                Verify(client.Token, link.Split('/').Last(), guildId);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Debug.Log(ex.StackTrace);
                                                        }
                                                    }
                                                    joined++;

                                                    verifyingCount--;
                                                    _progress.completed_tokens += 1;
                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                joined++;

                                                verifyingCount--;
                                                _progress.completed_tokens += 1;
                                                Debug.Log("Error during join - " + ex.StackTrace);
                                                c++;
                                                continue;
                                            }

                                            hasJoined = true;
                                            clients1[i] = null;
                                            clients[clients.IndexOf(client)] = null;
                                        }
                                        if (c > 0)
                                        {
                                            if (!hasJoined)
                                                _progress.completed_tokens += 1;

                                            clients1[i] = null;
                                            clients[clients.IndexOf(client)] = null;
                                            token_list.Remove(client.Token);
                                            continue;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Log(ex.Message);
                                    }
                                    i++;
                                    var to_sleep = delay - (DateTime.Now - joined_time).TotalMilliseconds;
                                    if (to_sleep < 0)
                                        to_sleep = 0;
                                    Thread.Sleep((int)to_sleep);
                                }
                            }
                            catch (InvalidOperationException ex) { Thread.Sleep(100); }
                        }
                        while (verifyingCount != 0)
                            Thread.Sleep(500);

                        Science.SendStatistic(ScienceTypes.join);
                        if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                            App.SendToWebhook(Settings.Default.Webhook, "Join task completed successfully\n**Server:** https://discord.gg/" + invite);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.mainWindow.ShowNotification("Join task completed successfully");
                        });
                    }
                    catch(Exception ex)
                    {
                        Debug.Log("Exception on joiner: " + ex.StackTrace);
                    }
                });
                joiner.Priority = ThreadPriority.AboveNormal;
                joiner.Start();

                var thread_pool = new List<Thread>() { };
                Thread join = new Thread(() =>
                {
                    try
                    {
                        int i = 0;
                        var threads = 0;
                        while (paused)
                        {
                            Thread.Sleep(1000);
                        }
                        var tk_list = token_list.ToArray();
                        var rnd = new Random();
                        foreach (var token in tk_list)
                        {
                            if (!joining || thread_pool.Count >= 20)
                                return;
                            Thread join1 = new Thread(() =>
                            {
                                if (!joining)
                                    return;
                                DiscordClient client;
                                try
                                {
                                    client = new DiscordClient(token);
                                    threads++;
                                }
                                catch (Exception ex)
                                {
                                    joined++;
                                    _progress.completed_tokens++;
                                    Debug.Log(ex.Message + "/// " + token);
                                    return;
                                }
                                if (proxyGroup != null && proxyGroup._proxies.Count > 0)
                                {
                                    var proxy = proxyGroup._proxies[rnd.Next(0, proxyGroup._proxies.Count)];
                                    if (proxy.Host != "" && proxy != null)
                                    {
                                        HttpProxyClient proxies = new HttpProxyClient(proxy.Host, proxy.Port);
                                        if (proxy.Username != null && proxy.Username != "")
                                            proxies = new HttpProxyClient(proxy.Host, proxy.Port, proxy.Username, proxy.Password);
                                        client.Proxy = proxies;
                                    }
                                }
                                if (!joining)
                                    return;
                                while (paused)
                                    Thread.Sleep(500);
                                if (IsInGuild(client, serverID) == true && !bypassReaction && !acceptRules && !bypassCaptcha)
                                {
                                    joined++;
                                    _progress.completed_tokens++;
                                    return;
                                }
                                else
                                {
                                    Debug.Log("Added client n." + tk_list.ToList().IndexOf(token));

                                    clients.Add(client);
                                }
                            });
                            thread_pool.Add(join1);
                            join1.Priority = ThreadPriority.AboveNormal;
                            join1.Start();
                            while (thread_pool.Count >= 20)
                            {
                                foreach (var thread in thread_pool)
                                {
                                    if (!thread.IsAlive)
                                    {
                                        thread_pool.Remove(thread);
                                        break;
                                    }
                                }
                                Thread.Sleep(500);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.Log("Exception on client adder: " + ex.StackTrace);
                    }
                });
                join.Start();
            }
            catch(Exception ex)
            {
                Debug.Log(ex.StackTrace);
                return;
            }

        }
        public static (string, string) Get_GuildID(string invite, ProxyGroup proxies = null)
        {
            string request_url = $"https://discord.com/api/v9/invites/{invite}";
            HttpRequest request = new HttpRequest()
            {
                KeepTemporaryHeadersOnRedirect = false,
                EnableMiddleHeaders = false,
                AllowEmptyHeaderValues = false
                //SslProtocols = SslProtocols.Tls12
            };
            if(proxies != null)
            {
                var rnd = new Random();
                var proxy = proxies._proxies[rnd.Next(0, proxies._proxies.Count - 1)];
                if (proxy.Username != null && proxy.Username != "")
                    request.Proxy = new HttpProxyClient(proxy.Host, proxy.Port, proxy.Username, proxy.Password);
                else
                    request.Proxy = new HttpProxyClient(proxy.Host, proxy.Port);

            }
            request.ClearAllHeaders();
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Accept-Language", "it");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
            request.AddHeader("DNT", "1");
            request.AddHeader("origin", "https://discord.com");
            request.AddHeader("Referer", "https://discord.com/channels/@me");
            request.AddHeader("TE", "Trailers");
            var response = request.Get(request_url);
            var resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
            if (((int)response.StatusCode) == 429)
            {
                return ("1", null);
            }
            var jtoken = JToken.Parse(response.ToString());
            var json = JObject.Parse(jtoken.ToString());
            try
            {
                string guild_id = json["guild"].Value<string>("id");
                string channel_id = json["channel"].Value<string>("id");
                return (guild_id, channel_id);
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }
        public bool? IsInGuild(DiscordClient Client, ulong guildId)
        {
            var tries = 0;
            if (Client.User.PhoneNumber == null)
                return false;
            while (tries < 3)
            {
                try
                {
                    foreach (var guild in Client.GetGuilds())
                    {
                        if (guildId == guild.Id)
                            return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("You need to verify"))
                        return true;
                    continue;
                }
            }
            return null;
        }
        public void Click_Button(DiscordClient client, DiscordMessage message)
        {
            string endpoint = "https://discord.com/api/v9/interactions";
            HttpRequest request = new HttpRequest()
            {
                KeepTemporaryHeadersOnRedirect = false,
                EnableMiddleHeaders = false,
                AllowEmptyHeaderValues = false
                //SslProtocols = SslProtocols.Tls12
            };
            request.Proxy = client.Proxy;
            /*
            request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port);
            if (_discordClient.Proxy.Username != null && _discordClient.Proxy.Username != "")
                request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port, _discordClient.Proxy.Username, _discordClient.Proxy.Password);
            */
            request.ClearAllHeaders();
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Accept-Language", "it");
            request.AddHeader("Authorization", client.Token);
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
            request.AddHeader("DNT", "1");
            request.AddHeader("origin", "https://discord.com");
            request.AddHeader("Referer", "https://discord.com/channels/@me");
            request.AddHeader("TE", "Trailers");
            request.AddHeader("User-Agent", client.Config.SuperProperties.UserAgent);
            request.AddHeader("x-fingerprint", "939165939738501131.C0ANMVTeVHCs9ZxYzLBcC8a36os");
            request.AddHeader("X-Super-Properties", client.Config.SuperProperties.ToBase64());

            HttpResponse response;
            var socketClient = new DiscordSocketClient();
            socketClient.Login(client.Token);
            while (!socketClient.LoggedIn)
                Thread.Sleep(10);
            var rnd = new Random();
            var salt = rnd.Next(10000, 99999).ToString();
            string json = "{" + $"\"type\":3,\"nonce\":\"9398285279632{salt}\",\"guild_id\":\"{serverID}\",\"channel_id\":\"{captchaChannelID}\",\"message_flags\":0,\"message_id\":\"{message.Id}\",\"application_id\":\"{message.Author.User.Id}\",\"session_id\":\"{socketClient.SessionId}\",\"data\":" + "{" + $"\"component_type\":2,\"custom_id\":\"{message.Components[0].Components[0].Id}\"" + "}}";
            request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
            response = request.Post(endpoint, json, "application/json");
            socketClient.Dispose();
            var resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
        }
        public string Click_Button_CaptchaBot(DiscordClient client, DiscordMessage message)
        {
            string endpoint = "https://discord.com/api/v9/interactions";
            HttpRequest request = new HttpRequest()
            {
                KeepTemporaryHeadersOnRedirect = false,
                EnableMiddleHeaders = false,
                AllowEmptyHeaderValues = false
                //SslProtocols = SslProtocols.Tls12
            };
            request.Proxy = client.Proxy;
            /*
            request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port);
            if (_discordClient.Proxy.Username != null && _discordClient.Proxy.Username != "")
                request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port, _discordClient.Proxy.Username, _discordClient.Proxy.Password);
            */
            request.ClearAllHeaders();
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Accept-Language", "it");
            request.AddHeader("Authorization", client.Token);
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
            request.AddHeader("DNT", "1");
            request.AddHeader("origin", "https://discord.com");
            request.AddHeader("Referer", "https://discord.com/channels/@me");
            request.AddHeader("TE", "Trailers");
            request.AddHeader("User-Agent", client.Config.SuperProperties.UserAgent);
            request.AddHeader("x-fingerprint", "939165939738501131.C0ANMVTeVHCs9ZxYzLBcC8a36os");
            request.AddHeader("X-Super-Properties", client.Config.SuperProperties.ToBase64());

            HttpResponse response;
            var socketClient = new DiscordSocketClient();
            socketClient.Login(client.Token);
            while (!socketClient.LoggedIn)
                Thread.Sleep(10);
            var rnd = new Random();
            var salt = rnd.Next(10000, 99999).ToString();
            string json = "{" + $"\"type\":3,\"nonce\":\"9398285279632{salt}\",\"guild_id\":\"{serverID}\",\"channel_id\":\"{captchaChannelID}\",\"message_flags\":0,\"message_id\":\"{message.Id}\",\"application_id\":\"{message.Author.User.Id}\",\"session_id\":\"{socketClient.SessionId}\",\"data\":" + "{" + $"\"component_type\":2,\"custom_id\":\"{message.Components[0].Components[0].Id}\"" + "}}";
            request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
            response = request.Post(endpoint, json, "application/json");
            var resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
            while (socketClient.WebSocket.captcha_bot_link == null)
            {
                Thread.Sleep(10);
            }
            return socketClient.WebSocket.captcha_bot_link;
        }
        public string GetCallBackURL(string token)
        {
            var payload = "{\"permissions\":\"0\",\"authorize\":true}";
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json"),
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://discord.com/api/v9/oauth2/authorize?client_id=512333785338216465&response_type=code&redirect_uri=https%3A%2F%2Fcaptcha.bot%2Fcallback&scope=identify%20guilds")
            }).GetAwaiter().GetResult();
            var json = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            return "https://captcha.bot/api/v1/oauth/callback?code=" + json.Value<string>("location").Split('=').Last();
        }
        public string GetAuthJWT(string token)
        {
            var client = new System.Net.Http.HttpClient();
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri(GetCallBackURL(token))
            }).GetAwaiter().GetResult();
            var json = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            return json.Value<string>("token");
        }
        public void Verify(string token, string hash, string guildID)
        {
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", GetAuthJWT(token));

            var payload = '{' + $"\"token\":\"{DiscordSolver.Solve("8223d1d4-b37a-46cc-b0e6-f9bf43658d5d")}\",\"hash\":\"{hash}\",\"guildID\":\"{guildID}\"" + '}';
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://captcha.bot/api/v1/captcha/verify"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
        }
        public void Stop()
        {
            joining = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.mainWindow.ShowNotification("Successfully stopped task");
            });
        }
        public void Pause()
        {
            paused = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.mainWindow.ShowNotification("Successfully paused task");
            });
        }
        public void Resume()
        {
            paused = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.mainWindow.ShowNotification("Successfully resumed task");
            });
        }
    }
}
