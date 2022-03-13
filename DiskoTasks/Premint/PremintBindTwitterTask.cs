using DiskoAIO.Premint;
using DiskoAIO.Properties;
using DiskoAIO.Twitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class PremintBindTwitterTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckAccount; }
            set { }
        }
        public string Type
        {
            get { return "Premint binder"; }
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
        private PremintAccountGroup _accountGroup;
        public PremintAccountGroup accountGroup
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
        public TwitterAccountGroup twitterAccountGroup;
        public int delay { get; set; } = 2;
        public bool checking = true;
        public bool paused = false;
        public PremintBindTwitterTask(PremintAccountGroup premintAccountGroup, TwitterAccountGroup twitterGroup)
        {
            accountGroup = premintAccountGroup;
            twitterAccountGroup = twitterGroup;
            _progress = new Progress(premintAccountGroup._accounts.Count < twitterAccountGroup._accounts.Count ? premintAccountGroup._accounts.Count : twitterAccountGroup._accounts.Count);
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                var max = accountGroup._accounts.Count < twitterAccountGroup._accounts.Count ? accountGroup._accounts.Count : twitterAccountGroup._accounts.Count;
                for (int i = 0; i < max; i++)
                {
                    try
                    {
                        var twitter = twitterAccountGroup._accounts[i];
                        accountGroup._accounts[i].ConnectTwitter(twitter.Username, twitter._password, twitter._phone);
                        _progress.Add(1);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                }
                Running = false;
                paused = false;
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Twitter binder generator task completed successfully\n**Group:** " + accountGroup._name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.premintAccountsView.ListTokens.Items.Refresh();
                    App.premintAccountsView.UpdateAccountCount();
                    App.mainWindow.ShowNotification("Twitter binder task completed successfully");
                });
                var index = App.premintGroups.IndexOf(accountGroup);

                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            using (var writer = new StreamWriter(App.strWorkPath + "\\premint\\" + App.premintGroups[index]._name + ".txt"))
                            {
                                foreach (var proxy in App.premintGroups[index]._accounts)
                                {
                                    writer.WriteLine(proxy.ToString());
                                }
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                        }
                    }
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
