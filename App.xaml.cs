using Discord;
using DiskoAIO.MVVM.View;
using DiskoAIO.Properties;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Leaf.xNet;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Net.Http;

namespace DiskoAIO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool connected_to_internet { get; set; } = false;
        public static string api_key { get; set; }
        public static string strExeFilePath { get; set; }

        public static string strWorkPath { get; set; }
        public static List<AccountGroup> accountsGroups { get; set; } = new List<AccountGroup>();
        public static List<ProxyGroup> proxyGroups { get; set; } = new List<ProxyGroup>();
        public static MainWindow mainWindow { get; set; }
        public static ProxiesView proxiesView { get; set; } = null;
        public static AccountsView accountsView { get; set; } = null;
        public static TasksView taskManager { get; set; } = null;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            strWorkPath = Path.GetDirectoryName(strExeFilePath);
            connected_to_internet = IsConnectedToInternet();
            SetAccountGroups();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            this.Dispatcher.UnhandledException += App_DispatcherUnhandledException;
            if (!IsServiceInstalled("DiskoUpdater"))
            {
                Debug.Log("Updater not installed");
                if (!IsUserAdministrator())
                {
                    var popup = new WarningPopupView("You need to run this program as administrator the first time running it", false);
                    popup.ShowDialog();
                    return;
                }
                if(!File.Exists(strWorkPath + "\\DiskoUpdater.exe"))
                {
                    var popup = new WarningPopupView("Couldn't find the updater, please reinstall the program or contact our support", false);
                    popup.ShowDialog();
                    return;
                }
                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "sc.exe",
                    Arguments = "CREATE \"DiskoUpdater\" binpath=" + $"\"{strWorkPath}\\DiskoUpdater.exe\"",
                });
                proc.WaitForExit();
                Debug.Log("Created service DiskoUpdater");
                proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "sc.exe",
                    Arguments = "config DiskoUpdater start=auto"
                });
                proc.WaitForExit();
                Debug.Log("Set service to start automatically");
                proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "sc.exe",
                    Arguments = "start DiskoUpdater"
                });
                proc.WaitForExit();
                Debug.Log("Started the updater");
            }
            if (Settings.Default.tk1 != "" && Settings.Default.tk2 != null)
            {
                var key = LoginWindow.login(Settings.Default.tk1, Settings.Default.tk2);
                if (key == null)
                {
                    Settings.Default.tk1 = "";
                    Settings.Default.tk2 = "";
                    var Login = new LoginWindow();
                    Login.ShowDialog();
                }
                else
                {
                    App.api_key = key;
                    Settings.Default.APIkey = key;

                    LoginWindow.BindMachine();
                    Settings.Default.Save();
                    Settings.Default.Reload();
                    App.SaveSettings();
                }
            }


            mainWindow = new MainWindow();
            mainWindow.Show();
        }
        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }
        public static void SendToWebhook(string webhook_link, string text)
        {
            var embed = new EmbedMaker() { Color = System.Drawing.Color.MediumPurple,
                Author = new EmbedAuthor()
                {
                    IconUrl = "https://cdn.discordapp.com/attachments/896745588505313280/930993204176756756/Logo.png",
                    Name = "DiskoAIO",
                    Url = "http://diskoaio.com"
                },
                Footer = new EmbedFooter()
                {
                    Text = "DiskoAIO"
                },
                Timestamp = DateTime.Now
            };
            embed.AddField("Winners:", text);
            ulong id = ulong.Parse(webhook_link.Split('/').Where(o => ulong.TryParse(o, out ulong any) == true).ToArray()[0]);
            var token = webhook_link.Split('/').Last();
            DiscordDefaultWebhook defaultWebhook = new DiscordDefaultWebhook(id, token);
            defaultWebhook.SendMessage("", embed);
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
                var tokens = new List<DiscordToken>();
                using (var reader = new StreamReader(file))
                {
                    var line = reader.ReadLine();
                    while (line != null && line != "")
                    {
                        try
                        {
                            line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                            var token_array = line.Split(':');
                            var token = DiscordToken.Load(token_array);
                            line = reader.ReadLine();
                            tokens.Add(token);
                        }
                        catch (FormatException ex)
                        {
                            ;
                        }
                    }
                }
                var group = new AccountGroup(tokens, name, false);
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
                var proxies = new List<DiscordProxy>();
                using (var reader = new StreamReader(file))
                {
                    var line = reader.ReadLine();
                    while (line != null && line != "")
                    {
                        try
                        {
                            line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                            var proxy_array = line.Split(':');
                            DiscordProxy proxy = null;
                            if (proxy_array.Length > 2)
                            {
                                if (int.TryParse(proxy_array[1], out var port))
                                    proxy = new DiscordProxy(proxy_array[0], port, proxy_array[2], proxy_array[3]);
                                else
                                    proxy = new DiscordProxy(proxy_array[2], int.Parse(proxy_array[3]), proxy_array[1], proxy_array[2]);
                            }
                            else
                            {
                                proxy = new DiscordProxy(proxy_array[0], int.Parse(proxy_array[1]));
                            }
                            line = reader.ReadLine();
                            proxies.Add(proxy);
                        }
                        catch (FormatException ex)
                        {
                            ;
                        }
                    }
                }
                var group = new ProxyGroup(proxies, name, false);
                if(group != null && group._name != "")
                    proxyGroups.Add(group);
            }
        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Debug.Log(e.Message + "///" + e.StackTrace);
            if (args.IsTerminating)
                DiscordDriver.CleanUp();
        }
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);    //we’ve reached the end of the tree
            if (parentObject == null) return null;
            //check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
        public static bool IsServiceInstalled(string serviceName)
        {
            // get list of Windows services
            ServiceController[] services = ServiceController.GetServices();

            // try to find service name
            foreach (ServiceController service in services)
            {
                if (service.ServiceName == serviceName)
                    return true;
            }
            return false;
        }
        public static void SaveSettings()
        {
            string exe_name = "DiskoAIO";

            XDocument xmlFile = XDocument.Load(strExeFilePath + ".config");

            foreach (XElement setting in xmlFile.Elements("configuration").Elements("userSettings").Elements($"{exe_name}.Settings").Elements("setting"))
            {
                switch (setting.Attribute("name").Value)
                {
                    case "AcceptRules":
                        setting.Element("value").Value = Settings.Default.AcceptRules.ToString();
                        break;
                    case "BypassReaction":
                        setting.Element("value").Value = Settings.Default.BypassReaction.ToString();
                        break;
                    case "CheckerGroup":
                        setting.Element("value").Value = Settings.Default.CheckerGroup;
                        break;
                    case "Delay":
                        setting.Element("value").Value = Settings.Default.Delay.ToString();
                        break;
                    case "ProxyGroup":
                        setting.Element("value").Value = Settings.Default.ProxyGroup;
                        break;
                    case "SendWebhook":
                        setting.Element("value").Value = Settings.Default.SendWebhook.ToString();
                        break;
                    case "tk1":
                        setting.Element("value").Value = Settings.Default.tk1;
                        break;
                    case "tk2":
                        setting.Element("value").Value = Settings.Default.tk2;
                        break;
                    case "TokenGroup":
                        setting.Element("value").Value = Settings.Default.TokenGroup;
                        break;
                    case "Type":
                        setting.Element("value").Value = Settings.Default.Type.ToString();
                        break;
                    case "UseProxies":
                        setting.Element("value").Value = Settings.Default.UseProxies.ToString();
                        break;
                    case "Webhook":
                        setting.Element("value").Value = Settings.Default.Webhook;
                        break;
                };
            }
            XElement xe = xmlFile.XPathSelectElement($"/userSettings/{exe_name}.Settings[1]");

            File.Delete(strWorkPath + @"\DiskoAIO.exe.config");
            xmlFile.Save(strWorkPath + @"\DiskoAIO.exe.config");
        }
        private static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
        public static bool CheckUpdate()
        {
            var xml = new List<string> { };
            foreach (XElement level1Element in XElement.Load(@"http://diskoaio.com/disko_update.xml").Elements("Binaries"))
            {
                foreach (XElement level2Element in level1Element.Elements("Binary"))
                {
                    if (level2Element.Attribute("name").Value == "DiskoUpdater.exe")
                        xml.Add(level2Element.Attribute("name").Value + ":" + level2Element.Attribute("version").Value);
                }
            }
            FileVersionInfo versionInfo = null;
            string version = "0.0.0.0";
            try
            {
                versionInfo = FileVersionInfo.GetVersionInfo(strWorkPath + @"\DiskoUpdater.exe");
                version = versionInfo.FileVersion;
            }
            catch (FileNotFoundException)
            {

            }
            version = version.Replace(".", string.Empty);
            var update_version = xml[0].Split(':')[1].Replace(".", string.Empty);
            if (int.Parse(version) < int.Parse(update_version))
            {
                string myWebUrlFile = "http://diskoaio.com/DiskoUpdater.exe";
                string myLocalFilePath = strWorkPath + @"\DiskoUpdater.exe";

                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (var client = new WebClient())
                {
                    if (!IsUserAdministrator())
                    {
                        MessageBox.Show("A new version of the updater is available. Please restart Tempo as administrator to install the new version");
                        Application.Current.Shutdown();
                        return true;
                    }
                    Process.Start("sc", "stop DiskoUpdater").WaitForExit();
                    Process.Start("sc", "delete DiskoUpdater").WaitForExit();

                    File.Delete(strWorkPath + @"\DiskoUpdater.exe");
                    try
                    {
                        client.DownloadFile(myWebUrlFile, myLocalFilePath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("You need to restart this program with admin privileges for it to update properly");
                        Console.ReadLine();
                        return false;
                    }
                    Process.Start("sc", "create DiskoUpdater binpath=\"" + strWorkPath + "\\DiskoUpdater.exe\"").WaitForExit();
                    Process.Start("sc", "start DiskoUpdater");
                }
            }
            return true;
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
