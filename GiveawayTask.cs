using Discord;
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
using System.Windows.Threading;

namespace DiskoAIO
{
    class GiveawayTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.Giveaway; }
            set { }
        }
        public string Type
        {
            get { return "Giveaway"; }
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
            get
            {
                if (_proxyGroup != null)
                    return _proxyGroup._name;
                else
                    return "null";
            }
        }
        public bool Running
        {
            get { return joining && !paused; }
            set { joining = value; paused = !value; }
        }
        public int delay { get; set; } = 2;
        private GiveawayType giveawayType { get; set; }
        ulong serverID { get; set; }
        ulong channelID { get; set; }
        ulong messageID { get; set; }
        int skip { get; set; }
        public bool joining { get; set; } = true;
        public bool paused { get; set; } = false;

        public void Start()
        {
            var clients = new List<DiscordClient>() { };
            var joined = 0;
            var token_list = new List<string>() { };
            foreach (var tk in accountGroup._accounts)
            {
                if (accountGroup._accounts.IndexOf(tk) < skip)
                    continue;
                if (token_list.Count >= accountGroup._accounts.Count)
                    break;
                token_list.Add(tk._token);
            }
            Thread joiner = new Thread(() =>
            {
                var joined_time = DateTime.Now;
                var messages = new List<DiscordMessage>();
                DiscordMessage mes = null;
                MessageReaction reaction = null;
                while (joining)
                {
                    while (paused)
                        Thread.Sleep(500);
                    try
                    {
                        if (joined == token_list.Count)
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
                                while (!hasJoined && c < 1)
                                {
                                    try
                                    {
                                        joined_time = DateTime.Now;

                                        if(giveawayType == GiveawayType.Reaction)
                                        {
                                            if(messages.Count == 0 || mes == null)
                                            {
                                                messages = (List<DiscordMessage>)client.GetChannelMessages(channelID, new MessageFilters()
                                                {
                                                    Limit = 50
                                                });
                                                foreach (var message in messages)
                                                {
                                                    if (message.Id != messageID)
                                                        continue;
                                                    mes = message;
                                                    var reactions = message.Reactions;
                                                    foreach(var react in reactions)
                                                    {
                                                        if (reaction == null)
                                                            reaction = react;
                                                        else
                                                        {
                                                            if (react.Count > reaction.Count)
                                                                reaction = react;
                                                        }
                                                    }
                                                }
                                            }
                                            try
                                            {
                                                client.AddMessageReaction(channelID, messageID, reaction.Emoji.Name, reaction.Emoji.Id);

                                            }
                                            catch { }
                                        }
                                        if (giveawayType == GiveawayType.Button)
                                        {
                                            if (messages.Count == 0 || mes == null)
                                            {
                                                bool clicked = false;
                                                messages = (List<DiscordMessage>)client.GetChannelMessages(channelID, new MessageFilters()
                                                {
                                                    Limit = 50
                                                });
                                                foreach (var message in messages)
                                                {
                                                    if (message.Id != messageID)
                                                        continue;
                                                    try
                                                    {
                                                        Click_Button(client, message);
                                                        clicked = true;
                                                    }
                                                    catch(Exception ex) { }
                                                }
                                                while(clicked != true)
                                                {
                                                    messages = (List<DiscordMessage>)client.GetChannelMessages(channelID, new MessageFilters()
                                                    {
                                                        Limit = 50,
                                                        BeforeId = messages.Last().Id
                                                    });
                                                    foreach (var message in messages)
                                                    {
                                                        if (message.Id != messageID)
                                                            continue;
                                                        try
                                                        {
                                                            Click_Button(client, message);
                                                            clicked = true;
                                                        }
                                                        catch (Exception ex) { }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _progress.completed_tokens += 1;

                                        c++;
                                        continue;
                                    }

                                    joined++;
                                    _progress.completed_tokens += 1;
                                    hasJoined = true;
                                    clients1[i] = null;
                                    clients[clients.IndexOf(client)] = null;
                                }
                                if (c >= 4)
                                {
                                    _progress.completed_tokens += 1;

                                    clients1[i] = null;
                                    clients[clients.IndexOf(client)] = null;
                                    token_list.Remove(client.Token);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Giveaway task completed successfully");
                });
            });
            joiner.Priority = ThreadPriority.AboveNormal;
            joiner.Start();

            var thread_pool = new List<Thread>() { };
            Thread join = new Thread(() =>
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
                        catch (Exception ex) { return; }
                        int tries = 0;
                        if (proxyGroup != null && proxyGroup._proxies.Count > 0)
                        {
                            while (tries <= 5)
                            {
                                var proxy = proxyGroup._proxies[rnd.Next(0, proxyGroup._proxies.Count)];
                                if (proxy.Host != "" && proxy != null)
                                {
                                    HttpProxyClient proxies = new HttpProxyClient(proxy.Host, proxy.Port);
                                    if (proxy.Username != null && proxy.Username != "")
                                        proxies = new HttpProxyClient(proxy.Host, proxy.Port, proxy.Username, proxy.Password);
                                    client.Proxy = proxies;
                                }
                                break;
                            }
                        }
                        clients.Add(client);

                    });
                    thread_pool.Add(join1);
                    join1.Priority = ThreadPriority.AboveNormal;
                    join1.Start();
                    while (thread_pool.Count >= 5)
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
            });
            join.Start();
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
        public GiveawayTask(AccountGroup accounts, ulong server_id, ulong channel_id, ulong message_id, ProxyGroup proxies = null, int _delay = 2, GiveawayType gtype = GiveawayType.Reaction, int skip_tokens = 0)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            delay = _delay;
            giveawayType = gtype;
            serverID = server_id;
            channelID = channel_id;
            messageID = message_id;
            skip = skip_tokens;

            progress = new Progress(accountGroup._accounts.Count);
        }

        private void Click_Button(DiscordClient client, DiscordMessage message)
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
            request.AddHeader("X-Super-Properties", client.Config.SuperProperties.ToBase64());

            HttpResponse response;
            string json = "{" + $"\"type\":3,\"nonce\":\"930838527963299840\",\"guild_id\":\"{serverID}\",\"channel_id\":\"{channelID}\",\"message_flags\":0,\"message_id\":\"{message.Id}\",\"application_id\":\"{message.Author.User.Id}\",\"session_id\":\"940e9ab61c31856394fff318ccbb06d8\",\"data\":"+ "{" + $"\"component_type\":2,\"custom_id\":\"{message.Components[0].Components[0].Id}\"" + "}}";
            request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
            response = request.Post(endpoint, json, "application/json");
            var resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
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
