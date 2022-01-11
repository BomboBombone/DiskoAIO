using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace DiskoAIO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string strWorkPath { get; set; }
        public static List<AccountGroup> accountsGroups { get; set; } = new List<AccountGroup>();
        public static List<ProxyGroup> proxyGroups { get; set; } = new List<ProxyGroup>();
        public static MainWindow mainWindow { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            strWorkPath = Path.GetDirectoryName(strExeFilePath);
            SetAccountGroups();

            mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void SetAccountGroups()
        {
            if (!Directory.Exists(strWorkPath + "/groups"))
            {
                Directory.CreateDirectory(strWorkPath + "/groups");
            }
            foreach(var file in Directory.GetFiles(strWorkPath + "/groups"))
            {
                var name = file.Split('\\').Last().Split('.')[0];

                var group = new AccountGroup(null, name, false);
                if (group != null && group._name != "")
                    accountsGroups.Add(group);
            }

            if (!Directory.Exists(strWorkPath + "/proxies"))
            {
                Directory.CreateDirectory(strWorkPath + "/proxies");
            }
            foreach (var file in Directory.GetFiles(strWorkPath + "/proxies"))
            {
                var name = file.Split('\\').Last().Split('.')[0];
                var group = new ProxyGroup(null, name, false);
                if(group != null && group._name != "")
                    proxyGroups.Add(group);
            }
        }
    }
    public enum GiveawayType
    {
        Button,
        Reaction
    }
    public enum TaskType
    {
        Join,
        Leave,
        Giveaway,
        CheckWin
    }
}
