using DiskoAIO.Properties;
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
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        public static string[] chatTypes = new string[] { "AI", "Message" };
        public ChatView()
        {
            InitializeComponent();
            ChatTypeGroup.ItemsSource = chatTypes;
            var source1 = new string[] { };
            foreach (var group in App.accountsGroups)
            {
                source1 = source1.Append(group._name).ToArray();
            }
            var source = new string[] { };
            foreach (var group in App.proxyGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            TokenGroup.ItemsSource = source1;
            TokenGroup.SelectedItem = Settings.Default.TokenGroup;
            ProxyGroup.ItemsSource = source;
            ProxyGroup.SelectedItem = Settings.Default.ProxyGroup;
            MinimumDelay.Text = Settings.Default.Delay.ToString();
            ChatTypeGroup.SelectedItem = chatTypes[0];
            ChatTypeGroup.SelectedItem = Settings.Default.ChatType;
            if (ChatTypeGroup.SelectedItem.ToString() == "AI")
            {
                ChatLabel.Content = "User ID";
                DelayBorder.Visibility = Visibility.Collapsed;
                MessagePathBorder.Visibility = Visibility.Collapsed;
                MaxTokensLabel.Content = "Reply rate (1-100)";
                TokensSkipLabel.Content = "Response rate (1-100)";
                GifsBorder.Visibility = Visibility.Visible;
                VerificationChannelBorder.Visibility = Visibility.Visible;
                LevelChannelBorder.Visibility = Visibility.Visible;
                MaxLvlBorder.Visibility = Visibility.Visible;
                InfiniteChatBorder.Visibility = Visibility.Collapsed;
                RotateBorder.Visibility = Visibility.Visible;
                if (RotateAccounts.IsChecked == true)
                {
                    ProxyLabel.Visibility = Visibility.Visible;
                    ProxyGroup.Visibility = Visibility.Visible;

                }
                else
                {
                    ProxyLabel.Visibility = Visibility.Collapsed;
                    ProxyGroup.Visibility = Visibility.Collapsed;

                }
                MaxTokens.Text = Settings.Default.AIReplyRate.ToString();
                SkipTokens.Text = Settings.Default.AIResponseRate.ToString();
            }
            else
            {
                ChatLabel.Content = "Channel ID";
                DelayBorder.Visibility = Visibility.Visible;
                MessagePathBorder.Visibility = Visibility.Visible;
                MaxTokensLabel.Content = "Max tokens";
                GifsBorder.Visibility = Visibility.Collapsed;
                TokensSkipLabel.Content = "Tokens to skip";
                VerificationChannelBorder.Visibility = Visibility.Collapsed;
                LevelChannelBorder.Visibility = Visibility.Collapsed;
                MaxLvlBorder.Visibility = Visibility.Collapsed;
                InfiniteChatBorder.Visibility = Visibility.Visible;
                RotateBorder.Visibility = Visibility.Collapsed;
            }
            if (Perpetual.IsChecked == true)
            {
                RepeatBorder.Visibility = Visibility.Visible;
            }
            else
            {
                RepeatBorder.Visibility = Visibility.Collapsed;

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

        private void ChatTypeGroup_DropDownClosed(object sender, EventArgs e)
        {
            if (ChatTypeGroup.SelectedItem == null)
                return;
            if(ChatTypeGroup.SelectedItem.ToString() == "AI")
            {
                ChatLabel.Content = "User ID";
                DelayBorder.Visibility = Visibility.Collapsed;
                MessagePathBorder.Visibility = Visibility.Collapsed;
                MaxTokensLabel.Content = "Reply rate (1-100)";
                GifsBorder.Visibility = Visibility.Visible;
                TokensSkipLabel.Content = "Response rate (1-100)";
                VerificationChannelBorder.Visibility = Visibility.Visible;
                LevelChannelBorder.Visibility = Visibility.Visible;
                MaxLvlBorder.Visibility = Visibility.Visible;
                InfiniteChatBorder.Visibility = Visibility.Collapsed;
                RotateBorder.Visibility = Visibility.Visible;

            }
            else
            {
                ChatLabel.Content = "Channel ID";
                DelayBorder.Visibility = Visibility.Visible;
                MessagePathBorder.Visibility = Visibility.Visible;
                MaxTokensLabel.Content = "Max tokens";
                GifsBorder.Visibility = Visibility.Collapsed;
                TokensSkipLabel.Content = "Tokens to skip";
                VerificationChannelBorder.Visibility = Visibility.Collapsed;
                LevelChannelBorder.Visibility = Visibility.Collapsed;
                MaxLvlBorder.Visibility = Visibility.Collapsed;
                InfiniteChatBorder.Visibility = Visibility.Visible;
                RotateBorder.Visibility = Visibility.Collapsed;

                if (Perpetual.IsChecked == true)
                {
                    RepeatBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    RepeatBorder.Visibility = Visibility.Collapsed;

                }
            }
        }

        private void Start_Task(object sender, RoutedEventArgs e)
        {
            if (TokenGroup.SelectedItem == null)
            {
                App.mainWindow.ShowNotification("Please select an account group");
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
            foreach (var group in App.proxyGroups)
            {
                if (ProxyGroup.SelectedItem != null && group._name == ProxyGroup.SelectedItem.ToString())
                {
                    proxies = group;
                    break;
                }
            }
            int skip = 0;
            int max = 0;
            int delay = Settings.Default.Delay;
            if (MinimumDelay.Text != "")
                delay = int.Parse(MinimumDelay.Text);
            if (SkipTokens.Text != "")
                skip = int.Parse(SkipTokens.Text);
            if (MaxTokens.Text != "")
                max = int.Parse(MaxTokens.Text);
            if (ulong.TryParse(UserID.Text, out var userId))
            {

            }
            if(MessagePath.Text == "" && ChatTypeGroup.SelectedItem.ToString() != "AI")
            {
                App.mainWindow.ShowNotification("Please select a file to take messages from");
                return;
            }
            if (ServerID.Text.Length != 18 || !ulong.TryParse(ServerID.Text, out var serverId))
            {
                App.mainWindow.ShowNotification("Please input a valid server ID");
                return;
            }
            if (ChannelID.Text.Length != 18 || !ulong.TryParse(ChannelID.Text, out var channelId))
            {
                App.mainWindow.ShowNotification("Please input a valid channel ID");
                return;
            }
            ulong lvlChannelId = 0;
            if ((LevelChannelID.Text.Length != 18 || !ulong.TryParse(LevelChannelID.Text, out lvlChannelId)) && LevelChannelID.Text != "")
            {
                App.mainWindow.ShowNotification("Please input a valid level channel ID");
                return;
            }
            if (ChatTypeGroup.SelectedItem == null)
            {
                App.mainWindow.ShowNotification("Please select the type of chat");
                return;
            }
            else if (ChatTypeGroup.SelectedItem.ToString() == "AI")
            {
                if (skip > 100 || skip < 1)
                {
                    App.mainWindow.ShowNotification("Invalid response rate, choose between 1% and 100%");
                    return;
                }

                try
                {
                    var task = new ChatBotTask(accounts, userId, serverId, channelId, skip, max, (bool)AllowLinks.IsChecked, lvlChannelId, int.Parse(MaxLvl.Text), (bool)RotateAccounts.IsChecked, proxies);
                    task.Start();
                    App.taskManager.AddTask(task);
                }
                catch(Exception ex)
                {
                    var task = new ChatBotTask(accounts, userId, serverId, channelId, skip, max, (bool)AllowLinks.IsChecked, 0, 0, (bool)RotateAccounts.IsChecked, proxies);
                    task.Start();
                    App.taskManager.AddTask(task);
                }
            }
            else
            {
                if (!File.Exists(MessagePath.Text))
                {
                    App.mainWindow.ShowNotification("Specified text file does not exist");
                    return;
                }
                int repeat = 0;
                if (RepeatAmount.Text != "")
                {
                    repeat = int.Parse(RepeatAmount.Text);
                }
                var task = new ChatTask(accounts, serverId, channelId, MessagePath.Text, delay, max, skip, (bool)Perpetual.IsChecked, repeat);
                task.Start();
                App.taskManager.AddTask(task);
            }
            App.mainWindow.ShowNotification("Task started successfully");
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

        private void RotateAccounts_Click(object sender, RoutedEventArgs e)
        {
            if(RotateAccounts.IsChecked == true)
            {
                ProxyLabel.Visibility = Visibility.Visible;
                ProxyGroup.Visibility = Visibility.Visible;

            }
            else
            {
                ProxyLabel.Visibility = Visibility.Collapsed;
                ProxyGroup.Visibility = Visibility.Collapsed;

            }
        }

        private void Perpetual_Click(object sender, RoutedEventArgs e)
        {
            if(Perpetual.IsChecked == true)
            {
                RepeatBorder.Visibility = Visibility.Visible;
            }
            else
            {
                RepeatBorder.Visibility = Visibility.Collapsed;

            }
        }
    }
}
