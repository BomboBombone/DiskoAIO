using Discord;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    class ImageChangerTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.ChangeImage; }
            set { }
        }
        public string Type
        {
            get { return "Image changer"; }
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
            get { return changing && !paused; }
            set { changing = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }

        public int delay { get; set; } = 2;

        public bool changing = true;
        public bool paused = false;
        public string path { get; set; }
        public ImageChangerTask(AccountGroup accountGroup, string folder_path)
        {
            _accountGroup = accountGroup;
            path = folder_path;
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
                    Bitmap avatar_bitmap = null;
                    while (true)
                    {
                        try
                        {
                            avatar_bitmap = (Bitmap)System.Drawing.Image.FromFile(image_path);
                            break;
                        }
                        catch (Exception ex)
                        {
                            images.Remove(path);

                            if (images.Count < 1)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    App.mainWindow.ShowNotification("No valid more valid images in selected folder, aborting task");
                                });
                                Running = false;
                                paused = false;
                                return;
                            }
                            path = images[rnd.Next(0, images.Count - 1)];
                        }
                    }
                    try
                    {
                        var client = new DiscordClient(account._token);
                        client.User.ChangeProfile(new UserProfileUpdate()
                        {
                            Avatar = avatar_bitmap
                        });
                        _progress.completed_tokens++;
                    }
                    catch(Exception ex)
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
            changing = false;
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
