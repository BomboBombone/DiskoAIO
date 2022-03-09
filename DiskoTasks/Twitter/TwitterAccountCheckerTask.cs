using DiskoAIO.Properties;
using DiskoAIO.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class TwitterAccountCheckerTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.Join; }
            set { }
        }
        public string Type
        {
            get { return "Follow Twitter"; }
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
            get { return checking && !paused; }
            set { checking = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }
        public int delay { get; set; } = 2;
        public int max_tokens { get; set; } = 0;
        int skip { get; set; }
        public bool checking { get; set; } = true;
        public bool paused { get; set; } = false;
        public TwitterAccountCheckerTask(TwitterAccountGroup accounts, ProxyGroup proxies = null)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            progress = new Progress(accounts._accounts.Count);
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                var validTokens = new List<Twitter.Twitter>();
                var original = accountGroup._accounts.Count;
                var rnd = new Random();

                foreach (var client in accountGroup._accounts)
                {
                    if (!checking)
                        break;
                    if (paused)
                        Thread.Sleep(100);
                    try
                    {
                        if(proxyGroup != null && proxyGroup._proxies.Count != 0)
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
                        }
                        client.Login();
                        if(client.IsValid())
                            validTokens.Add(client);
                    }
                    catch (Exception ex)
                    {

                    }
                    _progress.completed_tokens++;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (App.twitterAccountsView.ListTokens.SelectedItem != null && App.twitterAccountsView.ListTokens.SelectedItem.ToString() == accountGroup._name)
                    {
                        App.twitterAccountsView.ListTokens.ItemsSource = validTokens;
                        App.twitterAccountsView.ListTokens.Items.Refresh();
                    }
                    else
                    {
                        App.twitterGroups[App.twitterGroups.IndexOf(accountGroup)]._accounts = validTokens;
                        App.twitterAccountsView.ListTokens.ItemsSource = validTokens;
                        App.twitterAccountsView.ListTokens.Items.Refresh();
                    }
                    App.twitterAccountsView.UpdateAccountCount();
                    if (original - validTokens.Count > 0)
                        App.mainWindow.ShowNotification($"Successfully deleted {original - validTokens.Count} invalid or locked accounts, make sure to save to confirm changes");
                    else
                        App.mainWindow.ShowNotification($"No invalid or locked accounts were found");

                });
                Running = false;
                paused = false;
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Account checker task completed successfully\n**Group:** " + accountGroup._name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Account checker task completed successfully");
                });
            });
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
