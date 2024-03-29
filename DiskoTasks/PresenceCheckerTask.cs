﻿using Discord;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.DiskoTasks
{
    class PresenceCheckerTask : IDiskoTask
    {
        public TaskType type
        {
            get { return TaskType.CheckPresence; }
            set { }
        }
        public string Type
        {
            get { return "Presence checker"; }
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
        public ulong server_id;
        public PresenceType _type;
        public ulong roleID;
        public PresenceCheckerTask(AccountGroup accounts, ulong serverID, ProxyGroup proxies = null, PresenceType type = PresenceType.Presence, ulong roleId = 0)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            server_id = serverID;
            progress = new Progress(accounts._accounts.Count);
            _type = type;
            roleID = roleId;
        }
        public void Start()
        {
            Task.Run(() =>
            {
                Running = true;
                var validTokens = new List<DiscordToken>();
                foreach (var token in accountGroup._accounts)
                {
                    if (!checking)
                        break;
                    if (paused)
                        Thread.Sleep(100);
                    try
                    {
                        var client = new DiscordClient(token._token);

                        var user = client.GetGuild(server_id).GetMember(token._user_id);
                        if(_type == PresenceType.Role)
                        {
                            var found = false;
                            foreach(var role in user.Roles)
                            {
                                if (role == roleID)
                                    found = true;
                            }
                            if(!found)
                                throw new Exception();
                        }
                        validTokens.Add(token);
                    }
                    catch (Exception ex)
                    {

                    }
                    _progress.completed_tokens++;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if(_type == PresenceType.Presence)
                        App.mainWindow.ShowNotification($"{validTokens.Count} tokens are in the server {server_id}. Check your webhook for the full list (must enable extra webhook notifications in settings)!");
                    else
                        App.mainWindow.ShowNotification($"{validTokens.Count} tokens have role {roleID}. Check your webhook for the full list (must enable extra webhook notifications in settings)!");

                    if (Settings.Default.Webhook != "" && Settings.Default.SendWebhook)
                    {
                        string win = "\n";
                        var wins = new List<string>();
                        int i = 0;
                        foreach (var token in validTokens)
                        {
                            if (!checking)
                                return;
                            while (paused)
                                Thread.Sleep(500);
                            if (i == 8)
                            {
                                wins.Add(win);
                                win = "\n";
                                i = 0;
                            }
                            win += $"<@!{token._user_id}>\n||{token._token}||\n";
                            i++;
                        }
                        if (wins.Count < 1 || win != "\n")
                            wins.Add(win);
                        var client = new DiscordClient(validTokens.First()._token);

                        var guild = client.GetGuild(server_id);
                        foreach (string win_mes in wins)
                        {
                            if(_type == PresenceType.Presence)
                            {
                                App.SendToWebhook(Settings.Default.Webhook, $"Accounts inside server {guild.Name}:\n" + win_mes);

                            }
                            else
                            {
                                App.SendToWebhook(Settings.Default.Webhook, $"Accounts that have role {roleID}:\n" + win_mes);

                            }
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
