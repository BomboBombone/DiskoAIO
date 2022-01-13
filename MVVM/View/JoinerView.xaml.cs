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
    /// Interaction logic for JoinerView.xaml
    /// </summary>
    public partial class JoinerView : UserControl
    {
        public JoinerView()
        {
            InitializeComponent();
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
            TokenGroup.SelectedItem = Settings.Default.TokenGroup;
            ProxiesGroup.SelectedItem = Settings.Default.ProxyGroup;

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AcceptRules.IsChecked = Settings.Default.AcceptRules;
                            BypassReaction.IsChecked = Settings.Default.BypassReaction;
                            UseProxies.IsChecked = Settings.Default.UseProxies;
                        });
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1);
                    }
                }
            });

            VerificationChannelBorder.Visibility = Settings.Default.AcceptRules ? Visibility.Visible : Visibility.Collapsed;
            ProxiesGroup.Visibility = Settings.Default.UseProxies ? Visibility.Visible : Visibility.Collapsed;
            ProxiesLabel.Visibility = Settings.Default.UseProxies ? Visibility.Visible : Visibility.Collapsed;

            MinimumDelay.Text = Settings.Default.Delay.ToString();
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

        private void Join_Task(object sender, RoutedEventArgs e)
        {
            var info = Invite.Text.Split('/');
            if (Invite.Text == "")
            {
                App.mainWindow.ShowNotification("Please insert a valid message link");
                return;
            }

            AccountGroup accounts = null;
            foreach (var group in App.accountsGroups)
            {
                if (group._name == TokenGroup.SelectedItem.ToString())
                {
                    accounts = group;
                    break;
                }
            }
            if (accounts == null)
            {
                App.mainWindow.ShowNotification("Couldn't get specified account group, try again later");
                return;
            }
            ProxyGroup proxies = null;
            if ((bool)UseProxies.IsChecked)
            {
                foreach (var group in App.proxyGroups)
                {
                    if (group._name == ProxiesGroup.SelectedItem.ToString())
                    {
                        proxies = group;
                        break;
                    }
                }
                if (proxies == null)
                {
                    App.mainWindow.ShowNotification("Couldn't get specified proxy group, try again later");
                    return;
                }
            }
            int skip = 0;
            int delay = Settings.Default.Delay;
            if (MinimumDelay.Text != "")
                delay = int.Parse(MinimumDelay.Text);
            if (SkipTokens.Text != "")
                skip = int.Parse(SkipTokens.Text);
            ulong channelID = 0;
            if(ulong.TryParse(ChannelID.Text, out channelID))
            {

            }
            var joinerTask = new JoinTask(accounts, Invite.Text, channelID, proxies, delay, skip, (bool)AcceptRules.IsChecked, (bool)BypassReaction.IsChecked);
            joinerTask.Start();
            App.taskManager.AddTask(joinerTask);
            App.mainWindow.ShowNotification("Task started successfully");
        }
        private void Leave_Task(object sender, RoutedEventArgs e)
        {
            var info = Invite.Text.Split('/');
            if (Invite.Text == "")
            {
                App.mainWindow.ShowNotification("Please insert a valid message link");
                return;
            }

            AccountGroup accounts = null;
            foreach (var group in App.accountsGroups)
            {
                if (group._name == TokenGroup.SelectedItem.ToString())
                {
                    accounts = group;
                    break;
                }
            }
            if (accounts == null)
            {
                App.mainWindow.ShowNotification("Couldn't get specified account group, try again later");
                return;
            }
            ProxyGroup proxies = null;
            if ((bool)UseProxies.IsChecked)
            {
                foreach (var group in App.proxyGroups)
                {
                    if (group._name == ProxiesGroup.SelectedItem.ToString())
                    {
                        proxies = group;
                        break;
                    }
                }
                if (proxies == null)
                {
                    App.mainWindow.ShowNotification("Couldn't get specified proxy group, try again later");
                    return;
                }
            }
            int skip = 0;
            int delay = Settings.Default.Delay;
            if (MinimumDelay.Text != "")
                delay = int.Parse(MinimumDelay.Text);
            if (SkipTokens.Text != "")
                skip = int.Parse(SkipTokens.Text);
            ulong channelID = 0;
            if (ulong.TryParse(ChannelID.Text, out channelID))
            {

            }
            var leaveTask = new LeaveTask(accounts, Invite.Text, proxies, delay, skip);
            leaveTask.Start();
            App.taskManager.AddTask(leaveTask);
            App.mainWindow.ShowNotification("Task started successfully");
        }

        private void BypassReaction_Click(object sender, RoutedEventArgs e)
        {
            VerificationChannelBorder.Visibility = (bool)BypassReaction.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
        private void UseProxies_Click(object sender, RoutedEventArgs e)
        {
            ProxiesGroup.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ProxiesLabel.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;

        }
    }
}
