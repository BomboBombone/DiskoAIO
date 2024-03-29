﻿using Discord;
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
    class CheckerTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckWin; }
            set { }
        }
        public string Type
        {
            get { return "Giveaway checker"; }
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
                Running = true;
                DiscordSocketClient client = null;
                List<DiscordMessage> messages = new List<DiscordMessage>();
                foreach (var account in _accountGroup._accounts)
                {
                    if (!checking)
                        return;
                    while (paused)
                        Thread.Sleep(500);
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
                if(messages.Count == 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.mainWindow.ShowNotification("None of the tokens seem to have access to the specified message, try with another group");
                    });
                    return;
                }
                bool found = false;
                ulong[] user_ids = new ulong[] { };
                foreach (var message in messages)
                {
                    if (!checking)
                        return;
                    while (paused)
                        Thread.Sleep(500);
                    if (found)
                        break;
                    if (message.Id != messageID)
                        continue;
                    found = true;
                    var mes_arr = message.Content.Split('@');
                    foreach (var piece in mes_arr)
                    {
                        try
                        {
                            if (ulong.TryParse(piece.Substring(0, 19).Trim('!').Trim('>'), out var user_id))
                            {
                                user_ids = user_ids.Append(user_id).ToArray();
                            }
                        }
                        catch { continue; }
                    }
                    if(message.Embed != null)
                    {
                        if(message.Embed.Description != null)
                        {
                            mes_arr = message.Embed.Description.Split('@');
                            foreach (var piece in mes_arr)
                            {
                                try
                                {
                                    if (ulong.TryParse(piece.Substring(0, 19).Trim('!').Trim('>'), out var user_id))
                                    {
                                        user_ids = user_ids.Append(user_id).ToArray();
                                    }
                                }
                                catch { continue; }
                            }
                        }
                        if(message.Embed.Fields != null && message.Embed.Fields.Count > 0)
                        {
                            foreach(var field in message.Embed.Fields)
                            {
                                mes_arr = field.Content.Split('@');
                                foreach (var piece in mes_arr)
                                {
                                    try
                                    {
                                        if (ulong.TryParse(piece.Substring(0, 19).Trim('!').Trim('>'), out var user_id))
                                        {
                                            user_ids = user_ids.Append(user_id).ToArray();
                                        }
                                    }
                                    catch { continue; }
                                }
                            }
                        }
                    }
                }
                while (!found)
                {
                    if (!checking)
                        return;
                    while (paused)
                        Thread.Sleep(500);
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
                                if (ulong.TryParse(piece.Substring(0, 19).Trim('!').Trim('>'), out var user_id))
                                {
                                    user_ids = user_ids.Append(user_id).ToArray();
                                }
                            }
                            catch { continue; }
                        }
                    }
                }
                string win = "\n";
                var wins = new List<string>();
                int i = 0;
                foreach (var user_id in user_ids)
                {
                    if (!checking)
                        return;
                    while (paused)
                        Thread.Sleep(500);
                    foreach (var account in accountGroup._accounts)
                    {
                        if (account._user_id == user_id)
                        {
                            Science.SendStatistic(ScienceTypes.win);
                            if(i == 8)
                            {
                                wins.Add(win);
                                win = "\n";
                                i = 0;
                            }
                            win += $"<@!{account._user_id}>\n||{account._token}||\n";
                            i++;
                        }

                    }
                }
                if (wins.Count < 1 || win != "\n")
                    wins.Add(win);
                if (Settings.Default.Webhook != null)
                {
                    if (wins[0] != "\n")
                    {
                        foreach(string win_mes in wins)
                            App.SendToWebhook(Settings.Default.Webhook, win_mes, "https://discord.com/channels/" + serverID + '/' + channelID + '/' + messageID);
                    }
                    else
                        App.SendToWebhook(Settings.Default.Webhook, "No winners for this giveaway, you'll be lucky next time", "https://discord.com/channels/" + serverID + '/' + channelID + '/' + messageID);
                }
                Running = false;
                paused = false;
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
