using DeviceId;
using DiskoAIO.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static bool hasDisko { get; set; } = false;
        public LoginWindow()
        {
            InitializeComponent();
        }
        public static string login(string username, string password)
        {
            var request_url = $"https://diskoaio.com/api/v3/accounts?email={username}&password={password}";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = null;
            try
            {
                response = client.SendAsync(new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(request_url)
                }).GetAwaiter().GetResult();
            }
            catch
            {
                var popup = new WarningPopupView("Please make sure you're connected to internet before attempting to open DiskoAIO again");
                return null;
            }
            if (response.StatusCode.ToString() == "BadRequest")
                return null;
            if(response.StatusCode == System.Net.HttpStatusCode.BadGateway)
            {
                var popup = new WarningPopupView("Servers currently down, please contact support for more info");
                popup.ShowDialog();
                return null;
            }
            var jtoken = JToken.Parse(response.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jtoken.ToString());

            if (json.Value<string>("disko") == "1")
                hasDisko = true;
            else
                return null;

            return json.Value<string>("api_key");
        }
        public static void BindMachine(string mac_addr = null)
        {
            if (mac_addr == null || mac_addr == "")
                mac_addr = GetMacAddress();
            string address = "https://diskoaio.com/api/v3/accounts?mac=" + mac_addr;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.api_key);
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("PUT"),
                RequestUri = new Uri(address)
            }).GetAwaiter().GetResult();
            if (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
            {
                var request_url = $"https://diskoaio.com/api/v3/accounts?email={Settings.Default.tk1}&password={Settings.Default.tk2}";
                response = client.SendAsync(new HttpRequestMessage()
                {
                    Method = new System.Net.Http.HttpMethod("GET"),
                    RequestUri = new Uri(request_url)
                }).GetAwaiter().GetResult();
                if (response.StatusCode.ToString() == "BadRequest")
                {
                    Application.Current.Shutdown();
                    return;
                }
                var jtoken = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                var json = JObject.Parse(jtoken.ToString());
                if (json.Value<string>("mac") == mac_addr)
                    return;
                else
                {
                    Debug.Log("Binding found is invalid: " + mac_addr);
                    var popup = new WarningPopupView("Your licence key seems to be invalid, please try again or contact support");
                    popup.ShowDialog();

                    Settings.Default.tk1 = "";
                    Settings.Default.tk2 = "";
                    Settings.Default.APIkey = "";
                    Settings.Default.Save();
                    App.SaveSettings();

                    Application.Current.Shutdown();
                    return;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return;
            }
            else
            {
                MessageBox.Show("Could not get your MAC address, shutting down");
                Application.Current.Shutdown();
                Environment.Exit(1);
            }
        }
        private static string GetMacAddress()
        {
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();
            string cpuInfo = null;
            foreach (ManagementObject managObj in managCollec)
            {
                cpuInfo = managObj.Properties["processorID"].Value.ToString();
                break;
            }
            return cpuInfo;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            Application.Current.Dispatcher.InvokeShutdown();

            Environment.Exit(0);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if(UsernameIn.Text == "" || PasswordIn.Text == "")
            {
                return;
            }
            var key = login(UsernameIn.Text, PasswordIn.Text);
            if (key == null)
            {
                Settings.Default.tk1 = "";
                Settings.Default.tk2 = "";
                var popup = new WarningPopupView("Wrong credentials, please try again", false);
                popup.ShowDialog();
            }
            else
            {
                App.api_key = key;
                Settings.Default.APIkey = key;

                BindMachine();
                Settings.Default.tk1 = UsernameIn.Text;
                Settings.Default.tk2 = PasswordIn.Text;
                App.api_key = key;
                Settings.Default.Save();
                Settings.Default.Reload();
                App.SaveSettings();
                var popup = new WarningPopupView("Successfully claimed your licence", false);
                popup.ShowDialog();
                this.Close();
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
