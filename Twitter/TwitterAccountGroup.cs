using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiskoAIO.Twitter
{
    public class TwitterAccountGroup
    {
        public int _id { get; set; }
        public List<Twitter> _accounts { get; set; }
        public string _name { get; set; }
        public TwitterAccountGroup(List<Twitter> accounts, string name, bool createFile = true)
        {
            if (accounts != null)
                _accounts = accounts;
            else
                _accounts = new List<Twitter>();
            if (name == null || name == "")
                return;
            _id = 1;
            name = Regex.Replace(name, "[^A-Za-z0-9]", "") + ".txt";

            while (true)
            {
                try
                {
                    if (!Directory.Exists(App.strWorkPath + "\\twitter"))
                    {
                        Directory.CreateDirectory(App.strWorkPath + "\\twitter");
                    }
                    else
                    {
                        foreach (string file in Directory.GetFiles(App.strWorkPath + "\\twitter"))
                        {
                            if (file.Split('\\').Last() == name && createFile)
                                return;
                            _id += 1;
                        }
                    }
                    if (createFile)
                        File.Create(App.strWorkPath + "\\twitter\\" + name);

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
                    File.Delete(App.strWorkPath + "/twitter/" + _name + ".txt");
                    App.twitteGroups.Remove(this);
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
        public void Append(List<DiscordToken> to_append)
        {
            this._accounts.AddRange((IEnumerable<Twitter>)to_append);
        }
    }
}
