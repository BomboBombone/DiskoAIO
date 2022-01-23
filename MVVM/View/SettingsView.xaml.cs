using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            Type.ItemsSource = new string[] { "Reaction", "Button" };
            ChatTypeGroup.ItemsSource = ChatView.chatTypes;
            Task.Run(() =>
            {
                Thread.Sleep(50);
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BypassReaction.IsChecked = Settings.Default.BypassReaction;
                            AcceptRules.IsChecked = Settings.Default.AcceptRules;
                            TokenGroup.SelectedItem = Settings.Default.TokenGroup;
                            ProxiesGroup.SelectedItem = Settings.Default.ProxyGroup;
                            Webhook.Text = Settings.Default.Webhook;
                            UseProxies.IsChecked = Settings.Default.UseProxies;
                            SendInfoWebhook.IsChecked = Settings.Default.SendWebhook;
                            BypassCaptcha.IsChecked = Settings.Default.BypassCaptcha;
                            Anti_Captcha_Key.Text = Settings.Default.Anti_Captcha;
                            ResponseRate.Text = Settings.Default.AIResponseRate.ToString();
                            ReplyRate.Text = Settings.Default.AIReplyRate.ToString();
                            ChatTypeGroup.SelectedItem = Settings.Default.ChatType;
                        });
                        break;
                    }
                    catch(Exception ex)
                    {
                        Thread.Sleep(1);
                    }
                }
            });

            //SendInfoWebhook.IsChecked = Settings.Default.SendWebhook;
            Delay.Text = Settings.Default.Delay.ToString();
            CheckerTokenGroup.SelectedItem = Settings.Default.CheckerGroup;
            if(Settings.Default.Type == 0)
            {
                Type.SelectedItem = "Reaction";
            }
            else
            {
                Type.SelectedItem = "Button";
            }

            var source = new string[] { };
            foreach (var group in App.proxyGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            ProxiesGroup.ItemsSource = source;
            var source1 = new string[] { };
            foreach (var group in App.accountsGroups)
            {
                source1 = source1.Append(group._name).ToArray();
            }
            TokenGroup.ItemsSource = source1;
            CheckerTokenGroup.ItemsSource = source1;
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(!(int.TryParse(ReplyRate.Text, out var replyRate) && int.TryParse(ResponseRate.Text, out var responseRate)) || replyRate > 100 || responseRate > 100 || replyRate < 1 || responseRate < 1)
            {
                App.mainWindow.ShowNotification("Please select a response/reply rate between 1 and 100");
                return;
            }

            Settings.Default.BypassReaction = (bool)BypassReaction.IsChecked;
            Settings.Default.AcceptRules = (bool)AcceptRules.IsChecked;
            Settings.Default.TokenGroup = (string)TokenGroup.SelectedItem;
            Settings.Default.ProxyGroup = (string)ProxiesGroup.SelectedItem;
            Settings.Default.Webhook = Webhook.Text;
            Settings.Default.Delay = int.Parse(Delay.Text);
            Settings.Default.CheckerGroup = (string)CheckerTokenGroup.SelectedItem;
            Settings.Default.UseProxies = (bool)UseProxies.IsChecked;
            Settings.Default.SendWebhook = (bool)SendInfoWebhook.IsChecked;
            Settings.Default.BypassCaptcha = (bool)BypassCaptcha.IsChecked;
            Settings.Default.Anti_Captcha = Anti_Captcha_Key.Text;
            Settings.Default.AIReplyRate = int.Parse(ReplyRate.Text);
            Settings.Default.AIResponseRate = int.Parse(ResponseRate.Text);
            Settings.Default.ChatType = ChatTypeGroup.SelectedItem.ToString();
            Settings.Default.Save();
            Settings.Default.Reload();
            App.SaveSettings();
            App.mainWindow.ShowNotification("Successfully saved current settings");
        }
    }
}
