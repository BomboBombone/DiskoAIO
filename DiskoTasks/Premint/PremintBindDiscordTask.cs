using DiskoAIO.Premint;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class PremintBindDiscordTask : IDiskoTask
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
        public AccountGroup discordAccountGroup;
        public int delay { get; set; } = 2;
        public bool checking = true;
        public bool paused = false;
        public PremintBindDiscordTask(PremintAccountGroup premintAccountGroup, AccountGroup discordGroup)
        {
            accountGroup = premintAccountGroup;
            discordAccountGroup = discordGroup;
            _progress = new Progress(premintAccountGroup._accounts.Count > discordAccountGroup._accounts.Count ? premintAccountGroup._accounts.Count : discordAccountGroup._accounts.Count);

        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                var max = accountGroup._accounts.Count > discordAccountGroup._accounts.Count ? accountGroup._accounts.Count : discordAccountGroup._accounts.Count;
                for(int i = 0; i < max; i++)
                {
                    try
                    {
                        accountGroup._accounts[i].ConnectDiscord(discordAccountGroup._accounts[i]);
                        _progress.Add(1);
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                }
                Running = false;
                paused = false;
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Discord binder generator task completed successfully\n**Group:** " + accountGroup._name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.premintAccountsView.ListTokens.Items.Refresh();
                    App.premintAccountsView.UpdateAccountCount();
                    App.mainWindow.ShowNotification("Discord binder task completed successfully");
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
