using Discord;
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
    class LeaveTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.Leave; }
            set { }
        }
        public string Type
        {
            get { return "Leave"; }
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
        int skip { get; set; }
        public bool joining { get; set; } = true;
        public bool paused { get; set; } = false;
        public LeaveTask(AccountGroup accounts, string _invite, ProxyGroup proxies = null, int _delay = 2, int max = 0, int skip_tokens = 0)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            delay = _delay * 1000;
            if (max == 0)
                max = accounts._accounts.Count;
            max_tokens = max;
            skip = skip_tokens;
            invite = _invite;
            progress = new Progress(max);
        }
        public void Start()
        {
            if (invite.StartsWith("https://discord.gg/"))
                invite = invite.Remove(0, "https://discord.gg/".Length);
            if (invite.StartsWith("https://discord.com/invite/"))
                invite = invite.Remove(0, "https://discord.com/invite/".Length);
            var (guildId, channelWelcomeId) = JoinTask.Get_GuildID(invite);
            if (guildId == "1")
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
                if (accountGroup._accounts.IndexOf(tk) < skip)
                    continue;
                if (token_list.Count >= max_tokens)
                    break;
                token_list.Add(tk._token);
            }
            Thread joiner = new Thread(() =>
            {
                var joined_time = DateTime.Now;
                var messages = new List<DiscordMessage>();
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
                                while (!hasJoined && c < 1)
                                {
                                    try
                                    {
                                        joined_time = DateTime.Now;

                                        client.LeaveGuild(serverID);

                                    }
                                    catch (Exception ex)
                                    {
                                        _progress.completed_tokens += 1;
                                        Debug.Log(ex.Message);

                                        c++;
                                        continue;
                                    }
                                    _progress.completed_tokens += 1;

                                    joined++;
                                    hasJoined = true;
                                    clients1[i] = null;
                                    clients[clients.IndexOf(client)] = null;
                                }
                                if (c >= 1)
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
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Leave task completed successfully");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Leave task completed successfully");
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
                        if (!joining)
                            return;
                        while (paused)
                            Thread.Sleep(500);
                        if (IsInGuild(client, serverID) == false)
                        {
                            joined++;
                            return;
                        }
                        else
                        {
                            clients.Add(client);
                        }
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
        public bool? IsInGuild(DiscordClient Client, ulong guildId)
        {
            var tries = 0;
            if (Client.User.PhoneNumber == null)
                return true;
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
                        return false;
                    continue;
                }
            }
            return null;
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
