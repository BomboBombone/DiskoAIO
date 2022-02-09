using DiskoAIO.Properties;
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
using System.Windows.Shapes;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for CheckWinnersView.xaml
    /// </summary>
    public partial class CheckWinnersView : Window
    {
        public AccountGroup _currentGroup { get; set; } = null;

        public CheckWinnersView()
        {
            InitializeComponent();
            var source = new string[] { };

            foreach (var group in App.accountsGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            TokenGroup.ItemsSource = source;
            if (_currentGroup != null)
                TokenGroup.SelectedItem = _currentGroup._name;
            else
                if(App.accountsGroups.Count > 0)
                    _currentGroup = App.accountsGroups.First();
            TokenGroup.SelectedItem = Settings.Default.CheckerGroup;
            MessageLink.Focus();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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
            if(TokenGroup.SelectedItem == null)
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
            if(Settings.Default.Webhook == "")
            {
                App.mainWindow.ShowNotification("No webhook found, please insert one to use giveaway checker");
                return;
            }
            var checkerTask = new CheckerTask(accounts, server_id, channel_id, message_id);
            checkerTask.Start();
            App.taskManager.AddTask(checkerTask);
            App.mainWindow.ShowNotification("Task started successfully");
            this.Close();
        }
        private void StackPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox s = e.Source as TextBox;
                if (s != null)
                {
                    s.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }

                e.Handled = true;
            }
        }
    }
}
