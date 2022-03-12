using DiskoAIO.DiskoTasks;
using DiskoAIO.Premint;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for PremintAccountsView.xaml
    /// </summary>
    public partial class PremintAccountsView : UserControl
    {
        public PremintAccountsView()
        {
            InitializeComponent();
            if (App.premintAccountsView == null)
                App.premintAccountsView = this;
            var source = new string[] { };
            foreach (var group in App.premintGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            if (_currentGroup != null)
                GroupComboBox.SelectedItem = _currentGroup._name;
            else
                 if (App.premintGroups.Count > 0)
                    _currentGroup = App.premintGroups.First();
            GroupComboBox.SelectedItem = Settings.Default.TokenGroup;

            if (_currentGroup != null)
            {
                GroupComboBox.SelectedItem = _currentGroup._name;

                ListTokens.ItemsSource = _currentGroup._accounts;
            }
            UpdateAccountCount();
            if (seconds_remaining == 0)
                adding_accounts = false;
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification($"Still adding accounts, estimated time:\n{TimeSpan.FromSeconds(seconds_remaining).ToString()}", 2000);
            }
            if (!App.IsConnectedToInternet())
            {
                App.mainWindow.ShowNotification("You are not connected to internet");
            }
        }
        public int currentTokens { get; set; } = 0;
        public PremintAccountGroup _currentGroup { get; set; } = null;
        public static string to_search { get; set; } = "";
        public static bool adding_accounts { get; set; } = false;
        public static int seconds_remaining { get; set; } = 0;

        private void ListTokens_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (currentTokens > ListTokens.Items.Count)
                currentTokens = ListTokens.Items.Count;
            foreach (object item in ListTokens.Items)
            {
                currentTokens += 1;
            }
            TokenCounter.Content = "Accounts: " + currentTokens.ToString();
            App.twitterAccountsView.ListTokens.Items.Refresh();
        }
        private void Add_Group_Click(object sender, RoutedEventArgs e)
        {
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification("Cannot create group while adding tokens");
                return;
            }
            var inputName = new InputPopupView("Choose a name for the account group", 16);
            inputName.ShowDialog();
            var accountGroup = new PremintAccountGroup(null, inputName.answer);
            if (accountGroup == null || accountGroup._name == null)
            {
                App.mainWindow.ShowNotification("Invalid group name selected");
                return;
            }
            App.premintGroups.Add(accountGroup);
            _currentGroup = accountGroup;
            var source = new string[] { };
            foreach (var group in App.premintGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            GroupComboBox.SelectedIndex = source.Length - 1;
            UpdateAccountCount();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null)
            {
                App.mainWindow.ShowNotification("No group is selected for deletion");
                return;
            }
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification("Cannot delete group while adding tokens");
                return;
            }
            var popup = new WarningPopupView("You are about to delete group: " + _currentGroup._name + "\nAre you sure you want to continue?");
            popup.ShowDialog();
            if (!popup.hasConfirmed)
                return;
            var result = _currentGroup.Delete();
            if (result > 0)
                return;
            if (App.twitterGroups.Count > 0)
                _currentGroup = App.premintGroups.First();
            else
                _currentGroup = null;

            ListTokens.ItemsSource = new List<Premint.Premint>();
            ListTokens.Items.Refresh();

            var source = new string[] { };
            foreach (var group in App.premintGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            if (_currentGroup != null)
                GroupComboBox.SelectedItem = _currentGroup._name == null ? "" : _currentGroup._name;
            GroupComboBox.Items.Refresh();
            UpdateAccountCount();
        }
        public void UpdateAccountCount()
        {
            if (_currentGroup == null)
                TokenCounter.Content = "Accounts: 0";
            else
            {
                TokenCounter.Content = "Accounts: " + _currentGroup._accounts.Count.ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null || adding_accounts)
                return;
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using (var writer = new StreamWriter(App.strWorkPath + "\\premint\\" + _currentGroup._name + ".txt"))
                        {
                            foreach (var proxy in _currentGroup._accounts)
                            {
                                writer.WriteLine(proxy.ToString());
                            }
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        App.mainWindow.ShowNotification("Resource busy... waiting to save", 1000);

                        Thread.Sleep(1000);
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Successfully saved your current accounts");
                });
            });
        }

        private void Note_Double_Click(object sender, MouseButtonEventArgs e)
        {
            var source = new List<Premint.Premint>();
            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.Address.Contains(to_search))
                    source.Add(o);
            }

            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var input = new InputPopupView("Add your note here", 64, false, _currentGroup._accounts[index].Note);
            input.ShowDialog();
            if (input.answer == null)
            {
                App.mainWindow.ShowNotification("Invalid note, please try again", 1000);
                return;
            }
            if (input.answer == "")
                input.answer = "Double tap to add note...";

            source[index].Note = input.answer;
            ListTokens.ItemsSource = source;
            App.mainWindow.ShowNotification("Successfully saved note");
            ListTokens.Items.Refresh();
            TokenCounter.Content = ListTokens.Items.Count.ToString() + " accounts";
        }
        //private void Settings_Click(object sender, RoutedEventArgs e)
        //{
        //    var source = new List<Twitter.Twitter>();
        //    foreach (var o in _currentGroup._accounts)
        //    {
        //        if (o.Note.ToLower().Contains(to_search.ToLower()) &&
        //            o.Note != "Double click to add note..." &&
        //            to_search != "")
        //        {
        //            source.Add(o);
        //        }
        //        else if (o.Username.Contains(to_search))
        //            source.Add(o);
        //    }
        //
        //    var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
        //    var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
        //
        //    var popup = new AccountSettingsPopup(GroupComboBox.SelectedItem.ToString(), source[index], index);
        //    popup.Show();
        //}
        private void DeleteToken_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            _currentGroup._accounts.RemoveAt(index);
            ListTokens.ItemsSource = _currentGroup._accounts;
            ListTokens.Items.Refresh();
            UpdateAccountCount();
        }

        private void Search_Input(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                var source = new List<Premint.Premint>();
                foreach (var o in _currentGroup._accounts)
                {
                    if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                        o.Note != "Double click to add note..." &&
                        to_search != "")
                    {
                        source.Add(o);
                    }
                    else if (o.Address.Contains(to_search))
                        source.Add(o);
                }

                ListTokens.ItemsSource = source;
                ListTokens.Items.Refresh();
                UpdateAccountCount();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            to_search = ((TextBox)e.OriginalSource).Text;
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.SelectedItem == null)
            {
                return;
            }
            _currentGroup = App.premintGroups.Where(o => o._name == GroupComboBox.SelectedItem.ToString()).ToArray()[0];
            ListTokens.ItemsSource = _currentGroup._accounts;
            ListTokens.Items.Refresh();
            UpdateAccountCount();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var source = new List<Premint.Premint>();

            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.Address.Contains(to_search))
                    source.Add(o);
            }
            Clipboard.SetDataObject(source[index].Address);
            App.mainWindow.ShowNotification("Successfully copied public address");
        }

        private void Load_Tokens_Click(object sender, RoutedEventArgs e)
        {
            var popup = new InputPopupView("Enter the amount of accounts to create", 32, true);
            popup.ShowDialog();
            if(popup.answer != null && popup.answer != "")
            {
                var task = new PremintRegisterTask(_currentGroup, int.Parse(popup.answer));
                App.taskManager.AddTask(task);
                task.Start();
                App.mainWindow.ShowNotification("Successfully started generator task");
            }
        }

        private void Open_Browser(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            Task.Run(() =>
            {

                var source = new List<Premint.Premint>();

                foreach (var o in _currentGroup._accounts)
                {
                    if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                        o.Note != "Double click to add note..." &&
                        to_search != "")
                    {
                        source.Add(o);
                    }
                    else if (o.Address.Contains(to_search))
                        source.Add(o);
                }
                source[index].Login();
                new PremintDriver(source[index].Cookies.GetCookieHeader(new Uri("https://www.premint.xyz")));
            });
        }

        private void CopyPrivate_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var source = new List<Premint.Premint>();

            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.Address.Contains(to_search))
                    source.Add(o);
            }
            Clipboard.SetDataObject(source[index].private_key);
            App.mainWindow.ShowNotification("Successfully copied private key");
        }
    }
}
