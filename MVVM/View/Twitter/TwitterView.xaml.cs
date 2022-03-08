using DiskoAIO.DiskoTasks;
using DiskoAIO.Properties;
using DiskoAIO.Twitter;
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
    /// Interaction logic for TwitterView.xaml
    /// </summary>
    public partial class TwitterView : UserControl
    {
        public List<string> types = new List<string>() { "Follow", "Post" };
        public TwitterView()
        {
            InitializeComponent();
            Type.ItemsSource = types;
            Type.SelectedItem = types.First();
        }

        private void Start_Task(object sender, RoutedEventArgs e)
        {
            TwitterAccountGroup accounts = null;
            foreach (var group in App.twitterGroups)
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


            switch (Type.SelectedItem.ToString())
            {
                case "Follow":
                    if (!MessageLink.Text.StartsWith("https://twitter.com/"))
                    {
                        App.mainWindow.ShowNotification("Please insert a valid message link");
                        return;
                    }
                    var username = MessageLink.Text.Split('/').Last();
                    var task = new TwitterFollowTask(accounts, proxies, username);
                    task.Start();
                    App.taskManager.AddTask(task);

                    App.mainWindow.ShowNotification("Task started successfully");
                    break;
                case "Post":
                    ulong reply_to = 0;
                    if(ReplyTo.Text.Length == 19 && !ulong.TryParse(ReplyTo.Text, out reply_to))
                    {
                        App.mainWindow.ShowNotification("Please insert a valid message ID");
                        return;
                    }
                    var task1 = new TwitterPostTask(accounts, proxies, MessageLink.Text, reply_to);
                    task1.Start();
                    App.taskManager.AddTask(task1);

                    App.mainWindow.ShowNotification("Task started successfully");
                    break;
            }
        }

        private void Type_DropDownClosed(object sender, EventArgs e)
        {
            if(Type.SelectedItem.ToString() == "Follow")
            {
                MessageIDBorder.Visibility = Visibility.Collapsed;
                MessageBox.Content = "Profile link";
            }
            else if(Type.SelectedItem.ToString() == "Post")
            {
                MessageIDBorder.Visibility = Visibility.Visible;
                MessageBox.Content = "Message";
            }
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
            ProxiesGroup.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ProxiesLabel.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
