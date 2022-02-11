using DiskoAIO.DiskoTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for PresenceView.xaml
    /// </summary>
    public partial class PresenceView : UserControl
    {
        public PresenceView()
        {
            InitializeComponent();
        }

        private void Check_Task(object sender, RoutedEventArgs e)
        {
            if(!ulong.TryParse(ServerID.Text, out var serverID) || ServerID.Text.Length != 18)
            {
                App.mainWindow.ShowNotification("Please input a valid server ID");
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
            var checkPresenceTask = new PresenceCheckerTask(accounts, serverID, proxies);
            checkPresenceTask.Start();
            App.taskManager.AddTask(checkPresenceTask);
            App.mainWindow.ShowNotification("Successfully started presence checker task");
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }
        private void UseProxies_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)UseProxies.IsChecked)
            {
                ProxiesGroup.Visibility = Visibility.Visible;
                ProxiesLabel.Visibility = Visibility.Visible;
            }
            else
            {
                ProxiesGroup.Visibility = Visibility.Collapsed;
                ProxiesLabel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
