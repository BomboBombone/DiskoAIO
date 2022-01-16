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
    /// Interaction logic for AccountSettingsPopup.xaml
    /// </summary>
    public partial class AccountSettingsPopup : Window
    {
        public string originalGroup { get; set; }
        public int tokenIndex { get; set; } = 0;
        public DiscordToken _token { get; set; }
        public AccountSettingsPopup(string currentGroup, DiscordToken token, int index)
        {
            InitializeComponent();

            var source = new string[] { };
            foreach(var group in App.accountsGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            TokenGroup.ItemsSource = source;
            TokenGroup.SelectedItem = currentGroup;
            TokenGroup.Items.Refresh();

            TokenIn.Text = token._token;
            _token = token;
            originalGroup = currentGroup;
            tokenIndex = index;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(TokenGroup.SelectedItem.ToString() != originalGroup)
            {
                int ogIndex = 0;
                int ngIndex = 0;
                foreach(var group in App.accountsGroups)
                {
                    if(group._name == originalGroup)
                    {
                        break;
                    }
                    ogIndex++;
                }
                foreach (var group in App.accountsGroups)
                {
                    if (group._name == TokenGroup.SelectedItem.ToString())
                    {
                        break;
                    }
                    ngIndex++;
                }
                App.accountsGroups[ogIndex]._accounts.Remove(_token);
                App.accountsGroups[ngIndex]._accounts.Add(_token);
                var source = new List<DiscordToken>();
                foreach (DiscordToken o in App.accountsView.ListTokens.ItemsSource)
                {
                    if (o == _token)
                        continue;
                    if (o.Note.ToLower().Contains(AccountsView.to_search.ToLower()) &&
                        o.Note != "Double click to add note..." &&
                        AccountsView.to_search != "")
                    {
                        source.Add(o);
                    }
                    else if (o.User_id.Contains(AccountsView.to_search))
                        source.Add(o);
                }
                App.accountsView.ListTokens.ItemsSource = source;
                App.accountsView.ListTokens.Items.Refresh();
                App.mainWindow.ShowNotification("Successfully transfered token to group: " + TokenGroup.SelectedItem.ToString());
            }
            this.Close();
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

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_token._token);
        }
    }
}
