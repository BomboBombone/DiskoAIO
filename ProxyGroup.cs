﻿using DiskoAIO.DiskoTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiskoAIO
{
    public class ProxyGroup : IGroup
    {
        public int _id { get; set; }
        public List<DiscordProxy> _proxies { get; set; }
        public string _name { get; set; } = "";
        public ProxyGroup(List<DiscordProxy> proxies, string name, bool createFile = true)
        {
            if (proxies != null)
                _proxies = proxies;
            else
                _proxies = new List<DiscordProxy>();
            if (name == null || name == "")
                return;
            _id = 1;
            name = Regex.Replace(name, "[^A-Za-z0-9]", "") + ".txt";

            while (true)
            {
                try
                {
                    if (!Directory.Exists(App.strWorkPath + "\\proxies"))
                    {
                        Directory.CreateDirectory(App.strWorkPath + "\\proxies");
                    }
                    else
                    {
                        foreach (string file in Directory.GetFiles(App.strWorkPath + "\\proxies"))
                        {
                            if (file.Split('\\').Last() == name && createFile)
                                return;
                            _id += 1;
                        }
                    }
                    if(createFile)
                        File.Create(App.strWorkPath + "\\proxies\\" + name);

                    _name = name.Split('.')[0];

                    break;
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                    Thread.Sleep(1000);
                }
            }
        }
        public int Delete()
        {
            while (true)
            {
                try
                {
                    File.Delete(App.strWorkPath + "/proxies/" + _name + ".txt");
                    App.proxyGroups.Remove(this);
                    break;
                }
                catch (Exception ex)
                {
                    App.mainWindow.ShowNotification("Cannot delete group, try again in a couple seconds");
                    return 1;
                }
            }
            return 0;
        }
        public void Append(List<DiscordProxy> to_append)
        {
            this._proxies.AddRange(to_append);
        }
    }
}
