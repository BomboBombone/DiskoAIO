using Discord;
using Discord.Gateway;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    class ChatBotTask : DiskoTask
    {
        public TaskType type
        {
            get { return TaskType.ChatBot; }
            set { }
        }
        public string Type
        {
            get { return "Chat bot"; }
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
            get { return chatting && !paused; }
            set { chatting = value; paused = !value; }
        }
        public bool Paused
        {
            get { return paused; }
        }

        public int delay { get; set; } = 2;

        public bool chatting = true;
        public bool paused = false;
        public ulong userID { get; set; }
        public ulong serverID { get; set; }
        public ulong channelID { get; set; }
        private int answerRate { get; set; }
        private bool send_links { get; set; }
        public DiscordSocketClient client { get; set; }

        ulong chat_id = 0;
        int response_rate = 80;
        int error_count = 0;
        public ulong lvlChannelID = 0;
        public int maxLvl = 1;
        public BobbyAPI bobby = null;
        public bool _rotate = false;

        public ChatBotTask(AccountGroup accounts, ulong userId, ulong serverId, ulong channelId, int aggressivity = 80, int ans_rate = 60, bool allow_links = false, ulong lvlChanId = 0, int max = 1, bool rotate = false, ProxyGroup proxies = null)
        {
            accountGroup = accounts;
            proxyGroup = proxies;
            userID = userId;
            serverID = serverId;
            channelID = channelId;
            response_rate = aggressivity;
            answerRate = ans_rate;
            send_links = false;
            lvlChannelID = lvlChanId;
            maxLvl = max;
            _progress.total_tokens = max;
            _rotate = rotate;
        }
        public void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    Running = true;
                    HttpRequest request = new HttpRequest()
                    {
                        KeepTemporaryHeadersOnRedirect = false,
                        EnableMiddleHeaders = false,
                        AllowEmptyHeaderValues = false
                        //SslProtocols = SslProtocols.Tls12
                    };
                    try
                    {
                        var token = accountGroup._accounts.First(o => o.User_id == userID.ToString())._token;
                        bobby = new BobbyAPI(serverID);
                        if(bobby.chat_id == 0)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                App.mainWindow.ShowNotification("Couldn't get chats for Bobby, servers may be down...");
                            });
                            Running = false;
                            paused = false;
                            Debug.Log("Couldn't fetch a valid Bobby chat");
                            return;
                        }

                        client = new DiscordSocketClient(null, false, serverID, channelID, response_rate, lvlChannelID, maxLvl, bobby, _rotate, _rotate == true ? _accountGroup : null, _rotate == true ? _proxyGroup : null);

                        client.Login(token);
                        while (!client.LoggedIn)
                            Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.mainWindow.ShowNotification("Selected account couldn't connect, aborting chat task...");
                        });
                        Running = false;
                        paused = false;
                        Debug.Log(ex.Message);
                        return;
                    }

                    chat_id = 0;
                    client.OnMessageReceived += OnMessageReceived;
                    if (client.GetCachedGuild(serverID) == null)
                        client.GuildCache.Add(serverID, (SocketGuild)client.GetGuild(serverID));

                    while (Running || paused)
                    {
                        while (paused)
                            Thread.Sleep(10);
                        if (!client.isRunning)
                        {
                            Running = false;
                            paused = false;
                        }
                        if (progress.completed_tokens < client.currentLvl)
                            _progress.Add(1);
                        Thread.Sleep(1000);
                    }
                    client.Dispose();
                }
                catch(Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            });
        }

        private void OnMessageReceived(DiscordSocketClient client, MessageEventArgs args)
        {
            if (!Running)
            {
                return;
            }
            var start = DateTime.Now;
            client.answeringToMessage = true;
            var res = bobby.GetResponse(args.Message.Content);
            try
            {

                DiscordMessage msg = null;
                var end = DateTime.Now;
                if (args.Message.Channel.RateLimit != 0)
                    Thread.Sleep(args.Message.Channel.RateLimit * 1000 - (int)(end - start).TotalMilliseconds);
                else if ((end - start).TotalSeconds < 2)
                    Thread.Sleep(2000 - (int)(end - start).TotalMilliseconds);
                if (args.Message.Mentions != null && args.Message.Mentions.Contains(client.User))
                {
                    msg = MessageExtensions.SendMessage(client, args.Message.Channel.Id, new MessageProperties()
                    {
                        ReplyTo = new MessageReference(serverID, args.Message.Id),
                        Content = CleanString(res.ToString())
                    });
                }
                else
                {
                    var rnd = new Random();
                    if (rnd.Next(0, 100) < answerRate)
                    {
                        msg = MessageExtensions.SendMessage(client, args.Message.Channel.Id, new MessageProperties()
                        {
                            ReplyTo = new MessageReference(serverID, args.Message.Id),
                            Content = CleanString(res.ToString())
                        });
                    }
                    else
                    {
                        msg = MessageExtensions.SendMessage(client, args.Message.Channel.Id, new MessageProperties()
                        {
                            Content = CleanString(res.ToString())
                        });
                    }
                }
                if (msg == null)
                    error_count++;
                else
                    error_count = 0;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("diskoaio.com"))
                {
                    if (error_count > 2)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.mainWindow.ShowNotification("It seems like you had some problems connecting to diskoaio.com, aborting execution");
                        });
                        Running = false;
                        paused = false;
                    }
                    else
                        error_count++;
                }
                Debug.Log("Error during chatBot execution - " + ex.Message);
            }
            client.answeringToMessage = false;
        }
        private string CleanString(string input)
        {
            input = input.Replace('<', ' ').Replace('>', ' ').Replace('@', ' ').Trim('.');
            if (input.Contains("chat_id") || input.Contains("GG") || Regex.Replace(input, @"[^\u0000-\u007F]+", string.Empty) == "")
                return "Ayoo";
            if (input.ToLower().Contains("i'm from") || input.ToLower().Contains("im from") || input.ToLower().Contains("i am from"))
                return "I'm from Asia man";
            if (input.Contains("http") && !send_links)
                return "hm";
            foreach (var word in input.Split(' '))
            {
                try
                {
                    if (ulong.TryParse(word.Trim('!', '#'), out var userId))
                    {
                        return "Hi everyone new";
                    }
                }
                catch(Exception ex)
                {
                    continue;
                }
            }
            return Regex.Replace(input, @"[^\u0000-\u007F]+", string.Empty); ;
        }

        public void Stop()
        {
            chatting = false;
            paused = false;
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
