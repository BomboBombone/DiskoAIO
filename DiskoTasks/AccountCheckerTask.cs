using Discord;
using DiskoAIO.MVVM.View;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    class AccountCheckerTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckAccount; }
            set { }
        }
        public string Type
        {
            get { return "Account checker"; }
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

        public bool checking = true;
        public bool paused = false;

        public AccountCheckerTask(AccountGroup accounts, ProxyGroup proxies = null)
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
                var validTokens = new List<DiscordToken>();
                var original = accountGroup._accounts.Count;
                foreach (var token in accountGroup._accounts)
                {
                    if (!checking)
                        break;
                    if (paused)
                        Thread.Sleep(100);
                    try
                    {
                        var client = new DiscordClient(token._token);

                        client.QueryGuilds(new GuildQueryOptions()
                        {
                            Limit = 1
                        });
                        validTokens.Add(token);
                    }
                    catch (Exception ex)
                    {

                    }
                    _progress.completed_tokens++;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if(App.accountsView.ListTokens.SelectedItem != null && App.accountsView.ListTokens.SelectedItem.ToString() == accountGroup._name)
                    {
                        App.accountsView.ListTokens.ItemsSource = validTokens;
                        App.accountsView.ListTokens.Items.Refresh();
                    }
                    else
                    {
                        App.accountsGroups[App.accountsGroups.IndexOf(accountGroup)]._accounts = validTokens;
                        App.accountsView.ListTokens.ItemsSource = validTokens;
                        App.accountsView.ListTokens.Items.Refresh();
                    }
                    App.accountsView.UpdateAccountCount();
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
