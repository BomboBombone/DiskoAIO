using Discord;
using DiskoAIO.Properties;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class TwitterSniperTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.TwitterSniper; }
            set { }
        }
        public string Type
        {
            get { return "Twitter sniper"; }
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
            set
            {
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
                    return "None";
            }
        }
        public bool Running
        {
            get { return checking && !paused; }
            set { checking = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }

        public int delay { get; set; } = 2;

        public bool checking = true;
        public bool paused = false;
        public string twitter_username;
        public string lastTweetID = null;

        public TwitterSniperTask(AccountGroup accounts, string username, ProxyGroup proxies = null)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            progress = new Progress(0);
            twitter_username = username;
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                int sniped = 0;
                Twitter.Twitter twitter = null;
                try
                {
                    twitter = new Twitter.Twitter(twitter_username);

                }
                catch { }
                if(twitter == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.mainWindow.ShowNotification("Could not fetch the specified user, check that the user exists and you can connect to twitter.com");
                        return;
                    });
                }

                while (Running)
                {
                    while (paused)
                        Thread.Sleep(100);
                    var lastTweet = twitter.getLatestTweet();
                    if (lastTweetID == null)
                        lastTweetID = lastTweet.id;
                    if(lastTweetID != lastTweet.id)
                    {
                        var invite = ExtractInvite(lastTweet.text);
                        if(invite != null)
                        {
                            Debug.Log($"Found invite code {invite} from text {lastTweet.text}");
                            sniped++;
                            var (guildId, channelWelcomeId) = Get_GuildID(invite, proxyGroup);
                            if (guildId == "1" || guildId == null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    App.mainWindow.ShowNotification("IP banned from Discord, try again using proxies or a VPN");
                                });
                                checking = false;
                                return;
                            }
                            var serverID = ulong.Parse(guildId);
                            var clients = new List<DiscordClient>() { };
                            var joined = 0;
                            var token_list = new List<string>() { };
                            foreach (var tk in accountGroup._accounts)
                            {
                                if (tk == null)
                                    continue;
                                token_list.Add(tk._token);
                            }
                            Thread joiner = new Thread(() =>
                            {
                                try
                                {
                                    int verifyingCount = 0;
                                    var joined_time = DateTime.Now;
                                    var messages = new List<DiscordMessage>();

                                    while (checking)
                                    {
                                        while (paused)
                                            Thread.Sleep(500);
                                        int i = 0;
                                        try
                                        {
                                            var clients1 = new DiscordClient[clients.Count + 10];
                                            clients.CopyTo(clients1);
                                            foreach (var client in clients1)
                                            {
                                                if (!checking)
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
                                                            client.JoinGuild(invite);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _progress.completed_tokens += 1;
                                                            Debug.Log("Error during join - " + ex.StackTrace);
                                                            c++;
                                                            continue;
                                                        }

                                                        joined++;
                                                        hasJoined = true;
                                                        clients1[i] = null;
                                                        clients[clients.IndexOf(client)] = null;
                                                    }
                                                    if (c >= 1)
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
                                catch (Exception ex)
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
                                        if (!checking || thread_pool.Count >= 20)
                                            return;
                                        Thread join1 = new Thread(() =>
                                        {
                                            if (!checking)
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
                                            if (!checking)
                                                return;
                                            while (paused)
                                                Thread.Sleep(500);
                                            if (IsInGuild(client, serverID) == true)
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
                                catch (Exception ex)
                                {
                                    Debug.Log("Exception on client adder: " + ex.StackTrace);
                                }
                            });
                            join.Start();
                        }
                    }
                    Thread.Sleep(5000);
                }

                Running = false;
                paused = false;
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Invite sniper task has been stopped successfully\n**Group:** " + accountGroup._name + "\n**Sniped:** " + sniped.ToString());

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Invite sniper task has been stopped successfully");
                });
            });
        }

        private string ExtractInvite(string text)
        {
            foreach(var word in text.Split(' '))
            {
                var link = word;
                if(link.Contains("t.co"))
                {
                    var request = new HttpRequest()
                    {
                        AllowAutoRedirect = false
                    };
                    var res = request.Get(word);
                    try
                    {
                        link = res.Location;
                    }
                    catch { }
                }
                if (link.Contains("discord."))
                {
                    var code = link.Split('/').Last().Split('?').First();
                    code = new string(code.Where(o => Char.IsDigit(o) || Char.IsLetter(o)).ToArray());
                    return code;
                }
            }
            return null;
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
            if (proxies != null)
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
        public void Stop()
        {
            checking = false;
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
