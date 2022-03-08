using DiskoAIO.DiskoTasks;
using DiskoAIO.Properties;
using DiskoAIO.Twitter;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
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
                    var task = new TwitterFollowTask(accounts, proxies, username, delay, skip);
                    task.Start();
                    App.taskManager.AddTask(task);

                    App.mainWindow.ShowNotification("Task started successfully");
                    break;
                case "Post":
                    ulong reply_to = 0;
                    ulong to_tag = 0;
                    if(ReplyTo.Text.Length == 19 && !ulong.TryParse(ReplyTo.Text, out reply_to))
                    {
                        App.mainWindow.ShowNotification("Please insert a valid message ID");
                        return;
                    }
                    if(Tags.Text.Length > 0 && !ulong.TryParse(Tags.Text, out to_tag))
                    {
                        App.mainWindow.ShowNotification("Please insert a valid number of people to tag");
                        return;
                    }
                    if(UseFile.IsChecked == true && (!File.Exists(MessagePath.Text) || !MessagePath.Text.EndsWith(".txt")))
                    {
                        App.mainWindow.ShowNotification("Please insert a valid path to a text file");
                        return;
                    }
                    var task1 = new TwitterPostTask(accounts, proxies, MessageLink.Text, reply_to, AutoRetweet.IsChecked == true ? true : false, delay, skip, (int)to_tag, MessagePath.Text);
                    task1.Start();
                    App.taskManager.AddTask(task1);

                    App.mainWindow.ShowNotification("Task started successfully");
                    break;
            }
        }
        private void Explore_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select your message file";
            dialog.AddToMostRecentlyUsedList = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            string path = "";
            var result = CommonFileDialogResult.Ok;
            Dispatcher.Invoke(() => result = dialog.ShowDialog());
            if (result == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
                if (!path.EndsWith(".txt"))
                {
                    App.mainWindow.ShowNotification("Please select a .txt file");
                    return;
                }
                MessagePath.Text = path;
            }
        }
        private void Type_DropDownClosed(object sender, EventArgs e)
        {
            if(Type.SelectedItem.ToString() == "Follow")
            {
                MessageIDBorder.Visibility = Visibility.Collapsed;
                MessageBox.Content = "Profile link";
                if (UseFile.IsChecked != true)
                    MessagePathBorder.Visibility = Visibility.Collapsed;
                RetweetBorder.Visibility = Visibility.Collapsed;
                UseFileBorder.Visibility = Visibility.Collapsed;
                FriendTagBorder.Visibility = Visibility.Collapsed;
            }
            else if(Type.SelectedItem.ToString() == "Post")
            {
                MessageIDBorder.Visibility = Visibility.Visible;
                MessageBox.Content = "Message";

                if(UseFile.IsChecked == true)
                    MessagePathBorder.Visibility = Visibility.Visible;
                RetweetBorder.Visibility = Visibility.Visible;
                UseFileBorder.Visibility = Visibility.Visible;
                FriendTagBorder.Visibility = Visibility.Visible;
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

        private void UseFile_Click(object sender, RoutedEventArgs e)
        {
            if (UseFile.IsChecked == true)
                MessagePathBorder.Visibility = Visibility.Visible;
            else
                MessagePathBorder.Visibility = Visibility.Collapsed;

        }
    }
}
