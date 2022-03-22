using DiskoAIO.Properties;
using DiskoAIO.Twitter;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class TwitterPostTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.Join; }
            set { }
        }
        public string Type
        {
            get { return "Post Twitter"; }
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
        private TwitterAccountGroup _accountGroup;
        public TwitterAccountGroup accountGroup
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

        public int delay { get; set; } = 2;
        public int max_tokens { get; set; } = 0;
        int skip { get; set; }
        public bool joining { get; set; } = true;
        public bool paused { get; set; } = false;
        public ulong _reply_to;
        public string message;
        public bool _retweet;
        public int to_tag;
        public string path_to_file;
        public TwitterPostTask(TwitterAccountGroup accountGroup, ProxyGroup proxyGroup, string msg, ulong reply_to = 0, bool retweet = false, int _delay = 0, int _skip = 0, int number_to_tag = 0, string path = null)
        {
            _accountGroup = accountGroup;
            _proxyGroup = proxyGroup;
            message = msg;
            _reply_to = reply_to;
            _retweet = retweet;
            delay = _delay * 1000;
            skip = _skip;
            to_tag = number_to_tag;
            path_to_file = path;
            _progress = new Progress(_accountGroup._accounts.Count);
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;

                var clients = new List<Twitter.Twitter>() { };
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
                    token_list.Add(tk.ToString());
                }
                var messages = new List<string>();
                if(path_to_file != "")
                {
                    using (var reader = new StreamReader(path_to_file))
                    {
                        var line = reader.ReadLine();
                        while (line != null && line != "")
                        {
                            try
                            {
                                if (line == null)
                                    break;
                                line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                                messages.Add(line);
                            }
                            catch (FormatException ex)
                            {
                                Debug.Log(ex.Message);
                            }
                            line = reader.ReadLine();

                        }
                    }
                }
                Thread joiner = new Thread(() =>
                {
                    var joined_time = DateTime.Now;
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
                            var clients1 = new Twitter.Twitter[clients.Count + 10];
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
                                            try
                                            {
                                                if (!client.Cookies.GetCookieHeader(new Uri("https://twitter.com")).Contains("auth"))
                                                    client.Login();
                                            }
                                            catch (Exception ex)
                                            {
                                                client.Login();
                                            }
                                            var rnd = new Random();

                                            string new_mes = null;
                                            if(messages.Count != 0)
                                            {
                                                new_mes = messages[rnd.Next(0, messages.Count - 1)];
                                            }
                                            else
                                            {
                                                new_mes = message;
                                            }
                                            foreach(var name in accountGroup._accounts.OrderBy(x => rnd.Next()).Take(to_tag))
                                            {
                                                new_mes += $" @{name.Username}";
                                            }
                                            client.PostTweet(new_mes, _reply_to.ToString());
                                            if (_retweet)
                                                client.Retweet(_reply_to.ToString());
                                        }
                                        catch (Exception ex)
                                        {
                                            c++;
                                            continue;
                                        }

                                        joined++;
                                        _progress.completed_tokens += 1;
                                        hasJoined = true;
                                        clients1[i] = null;
                                        clients[clients.IndexOf(client)] = null;
                                    }
                                    if (c >= 1)
                                    {
                                        _progress.completed_tokens += 1;

                                        clients1[i] = null;
                                        clients[clients.IndexOf(client)] = null;
                                        token_list.Remove(client.ToString());
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
                    Science.SendStatistic(ScienceTypes.giveaway);
                    if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                        App.SendToWebhook(Settings.Default.Webhook, $"Twitter post task completed successfully\n");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.mainWindow.ShowNotification("Twitter post task completed successfully");
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
                            Twitter.Twitter client;
                            try
                            {
                                client = Twitter.Twitter.Load(token.Split(':').ToList());
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
                                        System.Net.WebProxy proxies = new System.Net.WebProxy($"http://{proxy.Host}:{proxy.Port}");
                                        if (proxy.Username != null && proxy.Username != "")
                                        {
                                            ICredentials credentials = new NetworkCredential(proxy.Username, proxy.Password);
                                            proxies = new WebProxy($"http://{proxy.Host}:{proxy.Port}", true, null, credentials);
                                        }
                                        if (client.clientHandler == null)
                                            client.InitializeHttpClient(proxies);
                                    }
                                    break;
                                }
                            }
                            if (!joining)
                                return;
                            while (paused)
                                Thread.Sleep(500);
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
            });
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
