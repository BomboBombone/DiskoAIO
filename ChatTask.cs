using Discord;
using Discord.Gateway;
using DiskoAIO.Properties;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    class ChatTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.ChatSpam; }
            set { }
        }
        public string Type
        {
            get { return "Message spam"; }
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
            get { return chatting && !paused; }
            set { chatting = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }

        public int delay { get; set; } = 2;

        public bool chatting = true;
        public bool paused = false;
        public ulong channelID { get; set; }
        public ulong serverID { get; set; }
        public bool persist { get; set; }
        string[] message_list { get; set; } = new string[] { };
        public int max_tokens { get; set; } = 0;
        public int skip { get; set; } = 0;
        public int _repeat_for;
        public ChatTask(AccountGroup accounts, ulong serverId, ulong channelId, string mes_path, int _delay = 2, int max = 0, int skip_tokens = 0, bool _persist = false, int repeat_for = 0)
        {
            accountGroup = accounts;
            serverID = serverId;
            channelID = channelId;
            delay = _delay * 1000;
            persist = _persist;
            using (var reader = new StreamReader(mes_path))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    message_list = message_list.Append(line).ToArray();
                    line = reader.ReadLine();
                }
            }
            max_tokens = max;
            if (max_tokens == 0)
                max_tokens = accountGroup._accounts.Count;
            skip = skip_tokens;
            if (!_persist)
                _progress = new Progress(max);
            else
                _progress = new Progress(0);
            _repeat_for = repeat_for;
        }
        public void Start()
        {
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
                    if (!chatting || thread_pool.Count >= 20)
                        return;
                    Thread join1 = new Thread(() =>
                    {
                        if (!chatting)
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
                        if (!chatting)
                            return;
                        while (paused)
                            Thread.Sleep(500);
                        if (IsInGuild(client, serverID) == false)
                        {
                            joined++;
                            _progress.completed_tokens++;
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
            Thread joiner = new Thread(() =>
            {
                var joined_time = DateTime.Now;
                var messages = new List<DiscordMessage>();
                int amount = 0;
                while (chatting)
                {
                    while (paused)
                        Thread.Sleep(500);
                    try
                    {
                        if (joined == max_tokens)
                            chatting = false;
                    }
                    catch (Exception ex) { }
                    int i = 0;
                    do
                    {
                        try
                        {
                            var clients1 = new DiscordClient[clients.Count + 10];
                            clients.CopyTo(clients1);
                            foreach (var client in clients1)
                            {
                                if (!chatting)
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
                                            var rnd = new Random();
                                            var message = message_list[rnd.Next(0, message_list.Length - 1)];
                                            var sent = MessageExtensions.SendMessage(client, channelID, new MessageProperties()
                                            {
                                                Content = message
                                            });
                                            if (sent == null)
                                            {
                                                try
                                                {
                                                    var socketClient = new DiscordSocketClient();
                                                    socketClient.Login(client.Token);
                                                    while (!socketClient.LoggedIn)
                                                        Thread.Sleep(10);
                                                    socketClient.Logout();
                                                    joined_time = DateTime.Now;
                                                    rnd = new Random();
                                                    message = message_list[rnd.Next(0, message_list.Length - 1)];
                                                    MessageExtensions.SendMessage(client, channelID, new MessageProperties()
                                                    {
                                                        Content = message
                                                    });
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Debug.Log(ex1.Message);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.Log(ex.Message);

                                            hasJoined = true;

                                            progress.Add(1);

                                            c++;
                                            continue;
                                        }
                                        if (!hasJoined)
                                            progress.Add(1);

                                        joined++;
                                        hasJoined = true;
                                    }
                                    if (c >= 1)
                                    {
                                        if (!hasJoined)
                                            progress.Add(1);

                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.Log(ex.Message);
                                }
                                i++;
                                clients.Remove(client);
                                token_list.Remove(client.Token);
                                var to_sleep = delay - (DateTime.Now - joined_time).TotalMilliseconds;
                                if (to_sleep < 0)
                                    to_sleep = 0;
                                Thread.Sleep((int)to_sleep);
                            }
                        }
                        catch (InvalidOperationException ex) { Thread.Sleep(100); }
                        if(clients.Count == token_list.Count && clients.Where(o => o != null).ToList().Count == 0)
                        {
                            amount++;
                            joined = 0;
                            thread_pool = new List<Thread>() { };
                            token_list = new List<string>() { };
                            foreach (var tk in accountGroup._accounts)
                            {
                                if (accountGroup._accounts.IndexOf(tk) < skip)
                                    continue;
                                if (token_list.Count >= max_tokens)
                                    break;
                                token_list.Add(tk._token);
                            }
                            clients = new List<DiscordClient>();
                            join = new Thread(() =>
                            {
                                i = 0;
                                var threads = 0;
                                while (paused)
                                {
                                    Thread.Sleep(1000);
                                }
                                var tk_list = token_list.ToArray();
                                var rnd = new Random();
                                foreach (var token in tk_list)
                                {
                                    if (!chatting || thread_pool.Count >= 20)
                                        return;
                                    Thread join1 = new Thread(() =>
                                    {
                                        if (!chatting)
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
                                        if (!chatting)
                                            return;
                                        while (paused)
                                            Thread.Sleep(500);
                                        if (IsInGuild(client, serverID) == false)
                                        {
                                            joined++;
                                            _progress.completed_tokens++;
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
                        if (amount == _repeat_for)
                            chatting = false;
                    }
                    while (persist && chatting);
                }
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Chat task completed successfully\n**Server ID:** " + serverID);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Chat task completed successfully");
                });
            });
            joiner.Priority = ThreadPriority.AboveNormal;
            joiner.Start();


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
            chatting = false;
            paused = false;
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
