using DiskoAIO.Premint;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class PremintRegisterTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckAccount; }
            set { }
        }
        public string Type
        {
            get { return "Account generator"; }
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
        public int delay { get; set; } = 2;
        public int max_accounts { get; set; }
        public bool checking = true;
        public bool paused = false;
        public PremintRegisterTask(PremintAccountGroup accounts, int max = 1)
        {
            accountGroup = accounts;
            max_accounts = max;
            _progress = new Progress(max);
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                var premint_accounts = new List<Premint.Premint>() { };
                for (int i = 0; i < max_accounts; i++)
                {
                    var premint = new Premint.Premint();
                    try
                    {
                        premint.Register();
                        premint_accounts.Add(premint);
                        _progress.Add(1);
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                }
                var index = App.premintGroups.IndexOf(accountGroup);
                App.premintGroups[index].Append(premint_accounts);


                Running = false;
                paused = false;
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Account (premint) generator task completed successfully\n**Group:** " + accountGroup._name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.premintAccountsView.ListTokens.Items.Refresh();
                    App.premintAccountsView.UpdateAccountCount();
                    App.mainWindow.ShowNotification("Account (premint) generator task completed successfully");
                });
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
