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
    /// Interaction logic for GiveawayView.xaml
    /// </summary>
    public partial class GiveawayView : UserControl
    {
        public GiveawayView()
        {
            InitializeComponent();
            Type.ItemsSource = new string[] { "Reaction", "Button" };
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
            if(Settings.Default.Type == 0)
                Type.SelectedItem =  "Reaction";
            else
                Type.SelectedItem = "Button";


            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
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

        private void Start_Task(object sender, RoutedEventArgs e)
        {
            var info = MessageLink.Text.Split('/');
            if (!(MessageLink.Text.Length == 85 &&
                ulong.TryParse(info.Last(), out var message_id) &&
                ulong.TryParse(info[info.Length - 2], out var channel_id) &&
                ulong.TryParse(info[info.Length - 3], out var server_id)))
            {
                App.mainWindow.ShowNotification("Please insert a valid message link");
                return;
            }
            if(ReactionID.Text == "")
            {
                App.mainWindow.ShowNotification("Please insert a valid reaction ID");
                return;
            }
            AccountGroup accounts = null;
            foreach(var group in App.accountsGroups)
            {
                if(group._name == TokenGroup.SelectedItem.ToString())
                {
                    accounts = group;
                    break;
                }
            }
            if(accounts == null)
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
            var type = GiveawayType.Reaction;
            if (Type.SelectedItem.ToString() == "Button")
                type = GiveawayType.Button;
            var giveawayTask = new GiveawayTask(accounts, server_id, channel_id, message_id, proxies, delay, type, skip, ReactionID.Text);
            giveawayTask.Start();
            App.taskManager.AddTask(giveawayTask);

            App.mainWindow.ShowNotification("Task started successfully");
        }

        private void Check_Winners(object sender, RoutedEventArgs e)
        {
            var winnerChecker = new CheckWinnersView();
            winnerChecker.Show();
        }

        private void UseProxies_Click(object sender, RoutedEventArgs e)
        {
            ProxiesGroup.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ProxiesLabel.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
