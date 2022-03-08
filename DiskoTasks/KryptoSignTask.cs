using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class KryptoSignTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.KryptoSign; }
            set { }
        }
        public string Type
        {
            get { return "KryptoSign"; }
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
        private AccountGroup _accountGroup = null;
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
        private ProxyGroup _proxyGroup = null;
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
            get { return running && !paused; }
            set { running = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }

        public int delay { get; set; }
        public bool running = true;
        public bool paused = false;
        public int max_tokens { get; set; }
        public int skip { get; set; }
        public string url { get; set; }
        public KryptoSignTask(AccountGroup accounts, ProxyGroup proxies, string _url, int _delay = 0, int max = 0, int to_skip = 0)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            delay = _delay * 1000;
            if (max == 0)
                max_tokens = accountGroup._accounts.Count;
            else
                max_tokens = max;
            skip = to_skip;
            url = _url;
            _progress = new Progress(max_tokens);
        }
        public void Start()
        {
            Running = true;
            Task.Run(() =>
            {
                var clients = new List<DiscordClient>() { };
                var token_list = new List<string>() { };
                foreach (var tk in accountGroup._accounts)
                {
                    if (accountGroup._accounts.IndexOf(tk) < skip)
                        continue;
                    if (token_list.Count >= max_tokens)
                        break;
                    token_list.Add(tk._token);
                }
                foreach(var tk in token_list)
                {
                    try
                    {
                        var client = new DiscordClient(tk);
                        if (!client.User.EmailVerified)
                            throw new Exception();
                        clients.Add(client);
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                }
                foreach (var client in clients)
                {
                    if (!Running)
                        break;
                    while (paused)
                    {
                        Thread.Sleep(100);
                    }
                    try
                    {
                        var krypto = new DiscordWeb3(client, url);
                        krypto.Complete();
                    }
                    catch(Exception ex)
                    {

                    }
                    _progress.Add(1);
                }
            });
        }

        public void Stop()
        {
            running = false;
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
