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
            TokenGroup.ItemsSource = source1;
            TokenGroup.SelectedItem = Settings.Default.TokenGroup;
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
            }
            else
            {
                ChatLabel.Content = "Channel ID";
                DelayBorder.Visibility = Visibility.Visible;
                MessagePathBorder.Visibility = Visibility.Visible;
                MaxTokensLabel.Content = "Max tokens";
                GifsBorder.Visibility = Visibility.Collapsed;
                TokensSkipLabel.Content = "Tokens to skip";
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
                App.mainWindow.ShowNotification("Please input a valid server ID");
                return;
            }
            if (!File.Exists(MessagePath.Text) && ChatTypeGroup.SelectedItem.ToString() != "AI")
            {
                App.mainWindow.ShowNotification("Specified text file does not exist");
                return;
            }
            if (ChatTypeGroup.SelectedItem == null)
            {
                App.mainWindow.ShowNotification("Please select the type of chat");
                return;
            }
            else if (ChatTypeGroup.SelectedItem.ToString() == "AI")
            {
                if(skip > 100 || skip < 1)
                {
                    App.mainWindow.ShowNotification("Invalid response rate, choose between 1% and 100%");
                    return;
                }
                var task = new ChatBotTask(accounts, userId, serverId, channelId, skip, max, (bool)AllowLinks.IsChecked);
                task.Start();
                App.taskManager.AddTask(task);
            }
            else
            {
                var task = new ChatTask(accounts, serverId, userId, MessagePath.Text, delay);
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
    }
}
