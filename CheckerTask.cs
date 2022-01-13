using Discord;
using Discord.Gateway;
using DiskoAIO.Properties;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    class CheckerTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckWin; }
            set { }
        }
        public string Type
        {
            get { return "Checker"; }
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
            get { return checking && !paused; }
            set { checking = value; paused = !value; }
        }

        public int delay { get; set; } = 2;
        ulong serverID { get; set; }
        ulong channelID { get; set; }
        ulong messageID { get; set; }

        public bool checking = true;
        public bool paused = false;

        public CheckerTask(AccountGroup accounts, ulong server_id, ulong channel_id, ulong message_id)
        {
            serverID = server_id;
            channelID = channel_id;
            messageID = message_id;
            accountGroup = accounts;
            channelID = channel_id;
            progress = new Progress(0);
        }
        public void Start()
        {
            if(Settings.Default.Webhook == "")
            {
                App.mainWindow.ShowNotification("No webhook selected, go in Settings and paste a webhook link");
                return;
            }
            Task.Run(() =>
            {
                DiscordSocketClient client = null;
                List<DiscordMessage> messages = new List<DiscordMessage>();
                foreach (var account in _accountGroup._accounts)
                {
                    client = new DiscordSocketClient(new DiscordSocketConfig()
                    {
                        ApiVersion = 9,
                        HandleIncomingMediaData = false
                    });
                    try
                    {
                        client.Login(account._token);
                        while (!client.LoggedIn)
                            Thread.Sleep(100);
                        messages = (List<DiscordMessage>)client.GetChannelMessages(channelID, new Discord.MessageFilters()
                        {
                            Limit = 50
                        });
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }
                if (client == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.mainWindow.ShowNotification("All tokens are invalid or IP banned from Discord, try again");
                    });
                    return;
                }
                bool found = false;
                ulong[] user_ids = new ulong[] { };
                foreach (var message in messages)
                {
                    if (message.Id != messageID)
                        continue;
                    found = true;
                    var mes_arr = message.Content.Split('@');
                    foreach (var piece in mes_arr)
                    {
                        try
                        {
                            if (ulong.TryParse(piece.Substring(0, 18), out var user_id))
                            {
                                user_ids = user_ids.Append(user_id).ToArray();
                            }
                            if (ulong.TryParse(piece.Substring(1, 19), out user_id))
                            {
                                user_ids = user_ids.Append(user_id).ToArray();
                            }
                        }
                        catch { continue; }
                    }
                }
                while (!found)
                {
                    messages = (List<DiscordMessage>)client.GetChannelMessages(channelID, new Discord.MessageFilters()
                    {
                        Limit = 50,
                        BeforeId = messages.Last().Id
                    });
                    foreach (var message in messages)
                    {
                        if (message.Id != messageID)
                            continue;
                        found = true;
                        var mes_arr = message.Content.Split('@');
                        foreach (var piece in mes_arr)
                        {
                            try
                            {
                                if (ulong.TryParse(piece.Substring(0, 18), out var user_id))
                                {
                                    user_ids = user_ids.Append(user_id).ToArray();
                                }
                            }
                            catch { continue; }
                        }
                    }
                }
                string win = "\n";

                foreach (var user_id in user_ids)
                {
                    foreach (var account in accountGroup._accounts)
                    {
                        if (account._user_id == user_id)
                        {
                            win += $"<@!{account._user_id}>\n||{account._token}||\n";
                        }
                    }
                }
                if (Settings.Default.Webhook != null)
                {
                    if (win != "\n")
                        App.SendToWebhook(Settings.Default.Webhook, win);
                    else
                        App.SendToWebhook(Settings.Default.Webhook, "No winners for this giveaway, you'll be lucky next time");
                }
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
