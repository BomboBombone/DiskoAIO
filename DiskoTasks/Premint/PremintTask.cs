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
    class PremintTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckAccount; }
            set { }
        }
        public string Type
        {
            get { return "Premint join"; }
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
            get { return running && !paused; }
            set { running = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }
        public int delay { get; set; } = 2;
        public bool running = true;
        public bool paused = false;
        public string project_name { get; set; }
        public bool solve_captcha { get; set; }
        public PremintTask(PremintAccountGroup premintAccountGroup, string name, bool captcha = false)
        {
            accountGroup = premintAccountGroup;
            project_name = name;
            solve_captcha = captcha;
            _progress = new Progress(accountGroup._accounts.Count);
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                foreach(var account in accountGroup._accounts)
                {
                    try
                    {
                        account.Login();
                        account.SubscribeToProject(project_name, solve_captcha);
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
                    App.SendToWebhook(Settings.Default.Webhook, "Premint task completed successfully\n**Group:** " + accountGroup._name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Premint task completed successfully");
                });
            });
        }
        public void Stop()
        {
            running = false;
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
