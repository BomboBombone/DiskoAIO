using DiskoAIO.Properties;
using DiskoAIO.Twitter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class TwitterImageChangerTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.Join; }
            set { }
        }
        public string Type
        {
            get { return "Twitter image changer"; }
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
        public bool joining { get; set; } = true;
        public bool paused { get; set; } = false;
        public int delay { get; set; }
        public string path { get; set; }
        public TwitterImageChangerTask(TwitterAccountGroup accountGroup, string folder_path)
        {
            _accountGroup = accountGroup;
            path = folder_path;

            _progress = new Progress(_accountGroup._accounts.Count);
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                string image_path = "";
                var images = new List<string>();
                try
                {
                    foreach (var file in Directory.GetFiles(path))
                    {
                        if (file.EndsWith(".png") || file.EndsWith(".jpg"))
                            images.Add(file);
                    }
                }
                catch (Exception ex) { }
                var rnd = new Random();
                foreach (var account in _accountGroup._accounts)
                {
                    image_path = images[rnd.Next(0, images.Count - 1)];
                    image_path = image_path.Replace('\\', '/');
                    try
                    {
                        account.Login();
                        account.ChangeImage(image_path);
                        _progress.completed_tokens++;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                }
                Running = false;
                paused = false;
                if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    App.SendToWebhook(Settings.Default.Webhook, "Image changer task completed successfully\n**Group:** " + accountGroup._name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Image changer task completed successfully");
                });
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
